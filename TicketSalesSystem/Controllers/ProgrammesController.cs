using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.Images;
using TicketSalesSystem.ViewModel.Programme;

namespace TicketSalesSystem.Controllers
{
    public class ProgrammesController : Controller
    {
        private readonly TicketsContext _context;
        

        public ProgrammesController(TicketsContext context)
        {
            _context = context;
        }

        // GET: Programmes
        public async Task<IActionResult> Index()
        {
            // 🚩 加入 OrderBy 確保每次讀取的順序一致，才不會覺得圖片跳來跳去
            var programme = await _context.Programme
                .Where(p => p.ProgrammeStatusID == "O") // 照你之前的需求，只看開賣中
                .OrderBy(p => p.ProgrammeID)            // 🚩 穩定排序
                .Select(p => new VMProgramme
                {
                    ProgrammeID = p.ProgrammeID,
                    ProgrammeName = p.ProgrammeName,
                    CoverImage = p.CoverImage, // 這裡拿的是資料庫字串，例如 "C20260101.jpg"

                    PlaceID = p.PlaceID,
                    PlaceName = p.Place != null ? p.Place.PlaceName : "尚未公佈地點",

                    ProgrammeStatusID = p.ProgrammeStatusID ?? "O",
                    ProgrammeStatusName = p.ProgrammeStatus != null ? p.ProgrammeStatus.ProgrammeStatusName : "售票中",

                    Capacity = p.Session.SelectMany(s => s.TicketsArea).Sum(a => a.Capacity),
                    Remaining = p.Session.SelectMany(s => s.TicketsArea).Sum(a => a.Remaining),

                    StartTime = p.Session.OrderBy(s => s.StartTime).Select(s => (DateTime?)s.StartTime).FirstOrDefault(),
                    SaleStartTime = p.Session.OrderBy(s => s.StartTime).Select(s => s.SaleStartTime).FirstOrDefault(),
                    SessionID = p.Session.OrderBy(s => s.StartTime).Select(s => s.SessionID).FirstOrDefault() ?? ""
                }).ToListAsync();

            return View(programme);
        }

        // GET: Programmes/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var programme = await _context.Programme                
                .Include(p => p.Place)
                .Include(p => p.ProgrammeStatus)
                .Include(p => p.Session) 
                .FirstOrDefaultAsync(m => m.ProgrammeID == id);
            if (programme == null)
            {
                return NotFound();
            }

            return View(programme);
        }

        public async Task<IActionResult> AdminIndex()
        {
            // 後台通常不進行狀態過濾 (Where)，以便管理所有活動
            var programmes = await _context.Programme
                .OrderByDescending(p => p.CreatedTime) // 照建立時間排序，新的在上面
                .Select(p => new VMProgramme
                {
                    ProgrammeID = p.ProgrammeID,
                    ProgrammeName = p.ProgrammeName,
                    CoverImage = p.CoverImage,
                    PlaceName = p.Place != null ? p.Place.PlaceName : "未設定",
                    ProgrammeStatusID = p.ProgrammeStatusID,
                    ProgrammeStatusName = p.ProgrammeStatus != null ? p.ProgrammeStatus.ProgrammeStatusName : "未知",
                    // 抓最早場次時間
                    StartTime = p.Session.OrderBy(s => s.StartTime).Select(s => (DateTime?)s.StartTime).FirstOrDefault(),
                    Capacity = p.Session.SelectMany(s => s.TicketsArea).Sum(a => a.Capacity),
                    Remaining = p.Session.SelectMany(s => s.TicketsArea).Sum(a => a.Remaining)
                    
                }).ToListAsync();

            return View(programmes);
        }





        // POST: Programmes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var programme = await _context.Programme.FindAsync(id);
            if (programme != null)
            {
                _context.Programme.Remove(programme);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

       
    }
}
