using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.ID;
using TicketSalesSystem.Service.Images;

namespace TicketSalesSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly TicketsContext _context;

        public AdminController(TicketsContext context)
        {
            _context = context;
            
        }
        public async Task<IActionResult> Dashboard()
        {
            //1. 總累計營收 (優化：處理 null 並確保是已付款訂單)
            ViewBag.TotalSales = await _context.Order
                .Where(o => o.PaymentStatus == true && o.OrderStatusID == "Y")
                .SelectMany(o => o.Tickets)
                .SumAsync(t => (decimal?)t.TicketsArea.Price) ?? 0;

            //2. 計算總銷售率 (已售張數 / 總容量)
            var allAreas = await _context.TicketsArea.Select(a => new { a.Capacity, a.Remaining }).ToListAsync();
            int totalCapacity = allAreas.Sum(a => a.Capacity);
            int totalSold = allAreas.Sum(a => a.Capacity - a.Remaining);

            ViewBag.TotalTicketsSold = totalSold;
            ViewBag.SalesRate = totalCapacity > 0 ? Math.Round((double)totalSold / totalCapacity * 100, 1) : 0;

            //3. 今日新增訂單 (過去 24 小時)
            var dayAgo = DateTime.Now.AddDays(-1);
            ViewBag.NewOrdersToday = await _context.Order
                .CountAsync(o => o.OrderCreatedTime >= dayAgo);

            // 4. 待處理客服
            var pendingStatuses = new[] { "N", "A" };
            ViewBag.PendingRefunds = await _context.Question
                .CountAsync(q =>q.Reply.Any()|| q.Reply.Any(r => pendingStatuses.Contains(r.ReplyStatusID)));

            // 5. 待處理活動、FAQ、公告、
            ViewBag.PendingProgramme = await _context.Programme
                .CountAsync(q => q.ProgrammeStatusID == "D" );//草稿

            ViewBag.PendingFAQ = await _context.FAQ
                .CountAsync(q => q.FAQPublishStatusID == "N");//未發佈

            ViewBag.PendingPublicNotice = await _context.PublicNotice
                .CountAsync(q => q.PublicNoticeStatus == false);//未發佈


            //6. 基礎統計
            ViewBag.MemberCount = await _context.Member.CountAsync();

            //7. 取得最近 5 筆訂單 (優化：Include 票區名稱以便顯示)
            var latestOrders = await _context.Order
                .Include(o => o.Member)
                .Include(o => o.OrderStatus)
                .Include(o => o.Tickets)
                    .ThenInclude(t => t.TicketsArea)
                .OrderByDescending(o => o.OrderCreatedTime)
                .Take(5)
                .ToListAsync();

            return View(latestOrders);
            
        }

        //專門給圖表使用，回傳 JSON
        [HttpGet]
        public async Task<IActionResult> GetAreaSalesData()
        {
            // 🚩 從 TicketsArea 出發，跨表抓到活動名稱
            var data = await _context.TicketsArea
                .Include(a => a.Session)
                    .ThenInclude(s => s.Programme)
                .GroupBy(a => a.Session.Programme.ProgrammeName) // 🚩 依活動名稱分組
                .Select(g => new {
                    label = g.Key, // 活動名稱
                    sold = g.Sum(x => x.Capacity - x.Remaining), // 該活動所有票區已售總數
                    remaining = g.Sum(x => x.Remaining)          // 該活動所有票區剩餘總數
                })
                .ToListAsync();

            return Json(data); // 直接回傳 JSON
        }
    }
}
