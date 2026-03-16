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
using TicketSalesSystem.Service.User;
using TicketSalesSystem.ViewModel.Programme;
using TicketSalesSystem.ViewModel.Programme.Index;
using TicketSalesSystem.ViewModel.Programme.ProgrammeAdminDetail;

namespace TicketSalesSystem.Controllers
{
    public class ProgrammesController : Controller
    {
        private readonly TicketsContext _context;
        private readonly IFileService _fileService;
        private readonly IUser _user;

        public ProgrammesController(TicketsContext context, IFileService fileService, IUser user)
        {
            _context = context;
            _fileService = fileService;
            _user = user;
        }

        
        // GET: Programmes
        public async Task<IActionResult> Index()
        {
            // 🚩 加入 OrderBy 確保每次讀取的順序一致，才不會覺得圖片跳來跳去
            var programme = await _user.GetProgrammesALL();

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
                .ThenInclude(s => s.TicketsArea)
                .FirstOrDefaultAsync(m => m.ProgrammeID == id);
            if (programme == null)
            {
                return NotFound();
            }

            return View(programme);
        }








        [Authorize(AuthenticationSchemes = "EmployeeScheme", Roles = "S,A,B")]

        // GET: Programmes/Details/5
        public async Task<IActionResult> AdminDetail(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            // 1. 一口氣抓出活動、場次、票區、狀態、地點與建立者
            var programme = await _context.Programme
                .Include(p => p.ProgrammeStatus)
                .Include(p => p.Place)
                .Include(p => p.Employee)
                .Include(p => p.DescriptionImage)
                .Include(p => p.Session)
                    .ThenInclude(s => s.TicketsArea)
                .FirstOrDefaultAsync(p => p.ProgrammeID == id);

            if (programme == null) return NotFound();

            // 2. 對應基本資料到 ViewModel
            var vm = new VMProgrammeAdminDetail
            {
                ProgrammeID = programme.ProgrammeID,
                ProgrammeName = programme.ProgrammeName,
                StatusName = programme.ProgrammeStatus?.ProgrammeStatusName,
                PlaceName = programme.Place?.PlaceName,
                ProgrammeDescription = programme.ProgrammeDescription,
                Notice = programme.Notice,
                RefundPolicy = programme.RefundPolicy,
                CoverImage = programme.CoverImage,
                SeatImage = programme.SeatImage,
                OnShelfTime = programme.OnShelfTime,
                UpdatedAt = programme.UpdatedAt ?? DateTime.Now,
                EmployeeName = programme.Employee?.Name,
                LimitPerOrder = programme.LimitPerOrder,
            };

            if (programme.DescriptionImage != null)
            {
                vm.DescriptionImages = programme.DescriptionImage.Select(di => new VMDescriptionImage
                {
                    // 🚩 這裡請對齊 VMDescriptionImage 的屬性名稱
                    ImagePath = di.ImagePath,
                    DescriptionImageName = di.DescriptionImageName
                }).ToList();
            }

            // 3. 核心：計算每個票區的即時銷售量 (Sold Count)
            foreach (var s in programme.Session)
            {
                var sessionVM = new VMSessionDetail
                {
                    SessionID = s.SessionID,
                    StartTime = s.StartTime,
                    SaleStartTime = s.SaleStartTime,
                    SaleEndTime = s.SaleEndTime,
                    TicketsAreas = new List<VMAreaDetail>()
                };

                foreach (var a in s.TicketsArea)
                {
                    // 🚩 從 Tickets 表中統計該票區已售出的張數
                    // 排除掉狀態為 'N' (已取消) 的訂單
                    int soldCount = await _context.Tickets
                        .CountAsync(t => t.TicketsAreaID == a.TicketsAreaID && t.Order.OrderStatusID != "N");

                    sessionVM.TicketsAreas.Add(new VMAreaDetail
                    {
                        TicketsAreaID = a.TicketsAreaID,
                        TicketsAreaName = a.TicketsAreaName,
                        Price = a.Price,
                        RowCount = a.RowCount,
                        SeatCount = a.SeatCount,
                        Capacity = a.Capacity,
                        Sold = soldCount,
                        Remaining = a.Remaining
                    });
                }
                vm.Sessions.Add(sessionVM);
            }

            return View(vm);
        }

        [Authorize(AuthenticationSchemes = "EmployeeScheme", Roles = "S,A,B")]
        public async Task<IActionResult> AdminIndex()
        {
            // 1. 抓取活動主檔 (包含導覽屬性以獲取地點與狀態)
            var programmes = await _context.Programme
                .AsNoTracking()
                .Include(p => p.Place)
                .Include(p => p.ProgrammeStatus)
                .OrderByDescending(p => p.ProgrammeID)
                .ToListAsync();

            // 2. 抓取所有場次與票區資料到記憶體，解決 8碼對10碼的匹配問題
            var allSessions = await _context.Session.AsNoTracking().ToListAsync();
            var allTickets = await _context.TicketsArea.AsNoTracking().ToListAsync();

            // 3. 投影組裝至 VMprogrammeIndex
            var vmList = programmes.Select(p => {

                // 🚩 核心：用 StartsWith 找出該活動的所有場次 (例如 20260303 開頭的 2026030301)
                var matchedSessions = allSessions
                    .Where(s => s.SessionID.Trim().StartsWith(p.ProgrammeID.Trim()))
                    .OrderBy(s => s.StartTime)
                    .ToList();

                // 🚩 根據場次 ID 找出所有的票區並計算總量
                var matchedSessionIDs = matchedSessions.Select(s => s.SessionID).ToList();
                var myTickets = allTickets.Where(t => matchedSessionIDs.Contains(t.SessionID)).ToList();

                return new VMprogrammeIndex
                {
                    ProgrammeID = p.ProgrammeID,
                    ProgrammeName = p.ProgrammeName,
                    CoverImage = p.CoverImage,
                    PlaceName = p.Place?.PlaceName ?? "尚未公佈地點",
                    ProgrammeStatusID = p.ProgrammeStatusID,
                    ProgrammeStatusName = p.ProgrammeStatus?.ProgrammeStatusName ?? "售票中",

                    // 填充場次清單
                    Session = matchedSessions.Select(s => new ProgrammeIndexSession
                    {
                        SessionID = s.SessionID,
                        StartTime = s.StartTime,
                        SaleStartTime = s.SaleStartTime,
                        SaleEndTime = s.SaleEndTime
                    }).ToList(),

                    Capacity = myTickets.Sum(t => (int?)t.Capacity) ?? 0,
                    Remaining = myTickets.Sum(t => (int?)t.Remaining) ?? 0
                };
            }).ToList();

            return View(vmList);
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
