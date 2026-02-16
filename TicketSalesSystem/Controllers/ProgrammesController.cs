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
            var programme = await _context.Programme
                .Select(p => new VMProgramme
                {
                    ProgrammeID = p.ProgrammeID,
                    CoverImage = p.CoverImage,
                    ProgrammeName = p.ProgrammeName,
                    // 🚩 修正 1：加上問號，如果 Place 是 null，則 PlaceName 為 null
                    PlaceName = p.Place != null ? p.Place.PlaceName : "未設定地點",

                    // 🚩 修正 2：最關鍵！先拿整個 Session 物件，再判斷 StartTime
                    // 或是使用 Null-conditional operator
                    StartTime = p.Session.OrderBy(s => s.StartTime).FirstOrDefault() != null
                        ? p.Session.OrderBy(s => s.StartTime).FirstOrDefault().StartTime
                        : null, // 注意：VMProgramme 的 StartTime 屬性必須改為 DateTime?// 注意：VMProgramme 的 StartTime 屬性必須改為 DateTime?

                    ProgrammeStatusName = p.ProgrammeStatus != null ? p.ProgrammeStatus.ProgrammeStatusName : "未知狀態",
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
                .Include(p => p.Employee)
                .Include(p => p.Place)
                .Include(p => p.ProgrammeStatus)
                .FirstOrDefaultAsync(m => m.ProgrammeID == id);
            if (programme == null)
            {
                return NotFound();
            }

            return View(programme);
        }



        private void PopulateDropdownLists(Programme p = null)
        {
            ViewData["EmployeeID"] = new SelectList(_context.Employee.ToList(), "EmployeeID", "Name", p?.EmployeeID);
            ViewData["PlaceID"] = new SelectList(_context.Place.ToList(), "PlaceID", "PlaceName", p?.PlaceID);
            ViewData["ProgrammeStatusID"] = new SelectList(_context.ProgrammeStatus.ToList(), "ProgrammeStatusID", "ProgrammeStatusName", p?.ProgrammeStatusID);
        }


       

        // GET: Programmes/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var programme = await _context.Programme
                .Include(p => p.Employee)
                .Include(p => p.Place)
                .Include(p => p.ProgrammeStatus)
                .FirstOrDefaultAsync(m => m.ProgrammeID == id);
            if (programme == null)
            {
                return NotFound();
            }

            return View(programme);
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
