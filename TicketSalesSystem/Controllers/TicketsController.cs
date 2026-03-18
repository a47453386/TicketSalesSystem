using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(AuthenticationSchemes = "EmployeeScheme", Roles = "S,A,B,C")]

    public class TicketsController : Controller
    {
        private readonly TicketsContext _context;

        public TicketsController(TicketsContext context)
        {
            _context = context;
        }

        


        // GET: Tickets
        public async Task<IActionResult> Index(string search, string status, string programmeId, string sessionId, int page = 1)
        {
            int pageSize = 50;

            // 🚩 1. 準備下拉選單資料 (用於分類過濾)
            ViewBag.Programmes = await _context.Programme
                .Select(p => new { p.ProgrammeID, p.ProgrammeName })
                .AsNoTracking().ToListAsync();

            // 如果有選擇活動，才抓取該活動的場次
            if (!string.IsNullOrEmpty(programmeId))
            {
                ViewBag.Sessions = await _context.Session
                    .Where(s => s.ProgrammeID == programmeId)
                    .Select(s => new { s.SessionID, s.StartTime })
                    .AsNoTracking().ToListAsync();
            }

            // 🚩 2. 建構基礎查詢
            var query = _context.Tickets
                .Include(t => t.TicketsArea)
                    .ThenInclude(a => a.Session)
                        .ThenInclude(p => p.Programme)
                .AsNoTracking()
                .AsQueryable();

            // 🚩 3. 執行過濾邏輯
            if (!string.IsNullOrEmpty(programmeId))
                query = query.Where(t => t.TicketsArea.Session.ProgrammeID == programmeId);

            if (!string.IsNullOrEmpty(sessionId))
                query = query.Where(t => t.SessionID == sessionId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.TicketsStatusID == status);

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(t => t.CheckInCode.Contains(search) ||
                                         t.OrderID.Contains(search) ||
                                         t.TicketsID.Contains(search));
            }

            // 🚩 4. 統計數據 (全域或過濾後，此處建議用全域)
            ViewBag.TotalCount = await _context.Tickets.CountAsync();
            ViewBag.UsedCount = await _context.Tickets.CountAsync(t => t.TicketsStatusID == "Y");
            ViewBag.RefundCount = await _context.Tickets.CountAsync(t => t.TicketsStatusID == "C");

            // 🚩 5. 分頁與結果
            var totalItems = await query.CountAsync();
            var results = await query
                .OrderByDescending(t => t.CreatedTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 🚩 6. 狀態回傳至 View
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentProgramme = programmeId;
            ViewBag.CurrentSession = sessionId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(results);
        }

        [HttpPost]
        public async Task<IActionResult> ManualCheckIn(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new { success = false, message = "無效的票券編號" });

            // 1. 尋找票券 (記得 Include Session，因為你要檢查場次日期)
            var ticket = await _context.Tickets
                .Include(t => t.Session)
                .FirstOrDefaultAsync(t => t.TicketsID == id);

            if (ticket == null)
                return Json(new { success = false, message = "找不到此票券資產" });

            // 2. 判斷狀態：是否重複核銷
            if (ticket.TicketsStatusID == "Y")
            {
                return Json(new
                {
                    success = false,
                    message = $"此票券已於 {ticket.ScannedTime:yyyy/MM/dd HH:mm:ss} 核銷過，請勿重複操作！"
                });
            }

            // 3. 判斷狀態：是否為有效票 (狀態 A)
            // 這樣可以一次擋掉「退票 C」、「尚未付款 L」或「取消 X」等非 A 的票
            if (ticket.TicketsStatusID != "A")
            {
                return Json(new { success = false, message = "此票券目前狀態無效（可能已退票、過期或尚未付款成功）" });
            }

            // 4. 判斷場次日期 (確保 Session 不為空)
            if (ticket.Session == null)
            {
                return Json(new { success = false, message = "系統錯誤：此票券無關聯的場次資訊，無法驗證日期" });
            }

            // 🚩 注意：如果你希望「後台管理員」有特權可以核銷「非今日」的票，
            // 可以把下面這段註解掉，或者是加一個權限判斷。
            if (ticket.Session.StartTime.Date != DateTime.Today)
            {
                return Json(new
                {
                    success = false,
                    message = $"核銷失敗！此票券非今日場次（場次日期：{ticket.Session.StartTime:yyyy/MM/dd}）"
                });
            }

            // 5. 執行核銷動作
            try
            {
                ticket.TicketsStatusID = "Y"; // 設為已使用
                ticket.ScannedTime = DateTime.Now; // 記錄核銷時間

                _context.Update(ticket);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"憑證 #{id.Substring(id.Length - 8).ToUpper()} 核銷成功！已授權入場"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "資料庫寫入失敗：" + ex.Message });
            }
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.TicketsStatus)
                .Include(t => t.TicketsArea)
                .Include(t => t.Session)
                    .ThenInclude(s => s.Programme)
                .Include(t => t.Order)
                    .ThenInclude(o => o.Member)
                .FirstOrDefaultAsync(m => m.TicketsID == id);

            if (ticket == null) return NotFound();

            return View(ticket);
        }


    }
}
