using Microsoft.AspNetCore.Authorization;
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
        private readonly IFileService _fileService;

        public ProgrammesController(TicketsContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
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

        [Authorize(AuthenticationSchemes = "EmployeeScheme", Roles = "S,A,B")]
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




        [Authorize(AuthenticationSchemes = "EmployeeScheme", Roles = "S,A,B")]
        // POST: Programmes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                // 1. 抓出活動，包含關聯的場次與票區
                var programme = await _context.Programme
                    .Include(p => p.Session)
                    .Include(p => p.DescriptionImage)
                    .FirstOrDefaultAsync(p => p.ProgrammeID == id);

                if (programme == null) return Json(new { success = false, message = "找不到該活動" });

                // 🚩 2. 安全檢查：如果已經有票賣出去了，禁止刪除 (這很重要！)
                bool hasSoldTickets = await _context.Tickets
                    .AnyAsync(t => t.TicketsArea.Session.ProgrammeID == id && t.Order.OrderStatusID != "N");

                if (hasSoldTickets)
                {
                    return Json(new { success = false, message = "此活動已有售票紀錄，無法刪除！建議改為下架狀態。" });
                }

                // 3. 備份要刪除的檔名（因為 DB 刪除後，物件內的資料會消失）
                string coverImgToDelete = programme.CoverImage;
                string seatImgToDelete = programme.SeatImage;
                var descImgsToDelete = programme.DescriptionImage.Select(di => di.DescriptionImageName).ToList();

                // 4. 執行資料庫刪除
                // 備註：請確保 DB 有設定 Cascade Delete，否則請手動移除相關 Session 與 DescriptionImage
                _context.Programme.Remove(programme);
                await _context.SaveChangesAsync();

                //5. 呼叫你的萬用服務刪除實體檔案 (非同步執行，不影響回傳速度)
                // 刪除封面圖
                await _fileService.DeleteFileAsync(coverImgToDelete,"Photos", "CoverImage");

                // 刪除座位圖
                await _fileService.DeleteFileAsync(seatImgToDelete, "Photos", "SeatImage");

                // 刪除多張說明圖
                foreach (var imgName in descImgsToDelete)
                {
                    await _fileService.DeleteFileAsync(imgName, "Photos", "DescriptionImage");
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "刪除失敗：" + ex.Message });
            }
        }

       
    }
}
