using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.ID;
using TicketSalesSystem.Service.Images;

namespace TicketSalesSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "EmployeeScheme")]
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



        //活動銷售分析
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


        //訂單健康度
        [HttpGet]
        public async Task<IActionResult> GetOrderStatusDistribution()
        {
            // 統計各個狀態的訂單張數
            var data = await _context.Order
                .GroupBy(o => o.OrderStatusID)
                .Select(g => new {
                    status = g.Key == "Y" ? "已付款" :
                             g.Key == "P" ? "待付款" :
                             g.Key == "N" ? "逾期付款" : "已失效",
                    count = g.Count()
                })
                .ToListAsync();

            return Json(data);
        }


        //銷售趨勢分析
        [HttpGet]
        public async Task<IActionResult> GetSalesVelocity()
        {
            var now = DateTime.Now;
            var last24Hours = now.AddHours(-23); // 從 23 小時前開始算起

            // 1. 抓出過去 24 小時的所有訂單
            var orders = await _context.Order
                .Where(o => o.OrderCreatedTime >= last24Hours)
                .Select(o => new { o.OrderCreatedTime })
                .ToListAsync();

            // 2. 建立連續的 24 小時時段清單，並與訂單數據媒合
            var velocityData = Enumerable.Range(0, 24).Select(offset =>
            {
                var hourPoint = last24Hours.AddHours(offset);
                var hourLabel = hourPoint.ToString("HH:00");

                // 統計該小時內的訂單數
                var count = orders.Count(o => o.OrderCreatedTime.Hour == hourPoint.Hour &&
                                              o.OrderCreatedTime.Date == hourPoint.Date);

                return new { hour = hourLabel, count = count };
            }).ToList();

            return Json(velocityData);
        }

        //即時警示與通知
        [HttpGet]
        public async Task<IActionResult> GetLiveAlerts()
        {
            // 1. 完售預警：剩餘數量 < 5% 且 尚未完全售罄 (Remaining > 0)
            var soldOutWarnings = await _context.TicketsArea
                .Include(a => a.Session).ThenInclude(s => s.Programme)
                .Where(a => a.Capacity > 0 && (double)a.Remaining / a.Capacity <= 0.05 && a.Remaining > 0)
                .Select(a => new {
                    Type = "SoldOut",
                    Title = "完售預警",
                    Message = $"{a.Session.Programme.ProgrammeName} - {a.TicketsAreaName} 剩餘不到 5%",
                    Level = "danger"
                })
                .ToListAsync();

            // 2. 物理上限警告：目前設定的 Capacity >= 該場館區域物理上限的 95%
            // 物理上限 = Venue.RowCount * Venue.SeatCount
            var physicalLimitWarnings = await _context.TicketsArea
                .Include(a => a.Venue)
                .Include(a => a.Session).ThenInclude(s => s.Programme)
                .Where(a => a.Venue != null && a.Capacity >= (a.Venue.RowCount * a.Venue.SeatCount) * 0.95)
                .Select(a => new {
                    Type = "PhysicalLimit",
                    Title = "物理上限警告",
                    Message = $"{a.TicketsAreaName} 設定張數 ({a.Capacity}) 已逼近場館物理上限 ({a.Venue.RowCount * a.Venue.SeatCount})",
                    Level = "warning"
                })
                .ToListAsync();

            // 合併所有警示
            var allAlerts = soldOutWarnings.Concat(physicalLimitWarnings).ToList();
            return Json(allAlerts);
        }
    }


}
