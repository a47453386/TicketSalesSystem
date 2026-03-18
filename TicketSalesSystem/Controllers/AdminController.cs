using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.ID;
using TicketSalesSystem.Service.Images;
using TicketSalesSystem.Service.SystemMonitor;
using TicketSalesSystem.Service.User;

namespace TicketSalesSystem.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(AuthenticationSchemes = "EmployeeScheme", Roles = "S,A,B,C,D,F")]
    public class AdminController : Controller
    {
        private readonly TicketsContext _context;
        private readonly SystemMonitorService _monitor;
        private readonly IUser _user;

        public AdminController(TicketsContext context, SystemMonitorService monitor,IUser user)
        {
            _context = context;
            _monitor = monitor;
            _user = user;
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
            ViewBag.PendingRefunds = await _context.Question
                .CountAsync(q => !q.Reply.Any(r => r.ReplyStatusID == "Y"));

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
                .Where(o => o.OrderStatusID == "Y" || o.OrderStatusID == "P" || o.OrderStatusID == "N")
                .GroupBy(o => o.OrderStatusID)
                .Select(g => new {
                    status = g.Key == "Y" ? "已付款" :
                             g.Key == "P" ? "待付款" :"逾期付款",                             
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
        // 即時銷售警示：僅監控庫存狀態
        [HttpGet]
        public async Task<IActionResult> GetLiveAlerts()
        {
            // 抓取所有剩餘量 <= 5% 的票區 (包含 0)
            var alerts = await _context.TicketsArea
                .Include(a => a.Session).ThenInclude(s => s.Programme)
                .Where(a => a.Capacity > 0 && (double)a.Remaining / a.Capacity <= 0.25)
                .Select(a => new {
                    a.TicketsAreaName,
                    ProgrammeName = a.Session.Programme.ProgrammeName,
                    a.Remaining,
                    a.Capacity
                })
                .ToListAsync();

            // 在記憶體中進行狀態分類，決定顯示內容
            var result = alerts.Select(a => new {
                Type = a.Remaining == 0 ? "SoldOut" : "Warning",
                Title = a.Remaining == 0 ? "已完售" : "完售預警",
                Message = a.Remaining == 0
                          ? $"{a.ProgrammeName} - {a.TicketsAreaName} 已全數售罄"
                          : $"{a.ProgrammeName} - {a.TicketsAreaName} 剩餘不到 5% (僅剩 {a.Remaining} 張)",
                Level = a.Remaining == 0 ? "secondary" : "danger" 
            }).ToList();

            return Json(result);
        }

        //取得背景日誌的頁面
        public IActionResult BackgroundLogs()
        {
            // 直接從 Singleton 服務拿取最新的記憶體日誌
            var logs = _monitor.GetLogs();
            return View(logs);
        }

        //如果你想讓頁面可以「局部刷新」或用 AJAX 抓日誌
        [HttpGet]
        public IActionResult GetLiveBackgroundLogs()
        {
            var logs = _monitor.GetLogs();
            var status = _monitor.CurrentStatus;

            return Json(new { logs, status });
        }

        public async Task<IActionResult> GetAttendanceData()
        {
            var data = await _context.Programme // 假設你的 Model 是 Programme
        .Select(p => new {
            // 🚩 這裡的屬性名稱會自動轉為 camelCase (例如 programmeName)
            programmeName = p.ProgrammeName,

            // 已進場人數 (狀態 Y)
            actualEntry = p.Session.SelectMany(s => s.Tickets)
                            .Count(t => t.TicketsStatusID == "Y"),

            // 未到場人數 (已售出狀態 A，但不包含已核銷 Y)
            notShow = p.Session.SelectMany(s => s.Tickets)
                        .Count(t => t.TicketsStatusID == "A")
        })
        .ToListAsync();

            return Json(data); 
        }


        [HttpGet]
        public async Task<IActionResult> GetLiveScanLogs()
        {
            // 🚩 從 Tickets 資料表撈取最近 10 筆已核銷的紀錄
            var logs = await _context.Tickets
                .Include(t => t.Session)
                .ThenInclude(s => s.Programme) // 關聯活動名稱
                .Where(t => t.TicketsStatusID == "Y" && t.ScannedTime != null)
                .OrderByDescending(t => t.ScannedTime) // 依時間降冪，最新的在上面
                .Take(10) // 每次取最新的 10 筆
                .Select(t => new {
                    // 🚩 格式化輸出給前端 JS 讀取
                    time = t.ScannedTime.Value.ToString("HH:mm:ss"),
                    code = t.CheckInCode,
                    programme = t.Session.Programme.ProgrammeName
                })
                .ToListAsync();

            return Json(logs);
        }


        //票區位子監控
        public async Task<IActionResult> AreaMonitor(string programmeId)
        {
            if (string.IsNullOrEmpty(programmeId)) return RedirectToAction("ActiveMonitorList");

            // 1. 先抓出該活動的所有場次 (Session)
            var sessions = await _context.Session
                .Where(s => s.ProgrammeID == programmeId)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
            var sessionIds = sessions.Select(s => s.SessionID).ToList();

            // 2. 🚩 修正：透過場次 ID 去找出「有哪些票區」
            // 這裡假設 Tickets 表連結了 Session 與 TicketsArea
            var areaDefinitions = await _context.TicketsArea
            .Where(a => sessionIds.Contains(a.SessionID))
            .OrderBy(a => a.TicketsAreaID)
            .ToListAsync();

            ViewBag.Sessions = sessions;
            ViewBag.Areas = areaDefinitions; // 這裡現在保證有 4 個區（只要資料庫有定義）
            ViewBag.ProgrammeName = _context.Programme.FirstOrDefault(p => p.ProgrammeID == programmeId)?.ProgrammeName;

            // 3. 抓取所有已售/變動的票券紀錄 (填色用資料)
            var allTickets = await _context.Tickets
                .Where(t => sessionIds.Contains(t.SessionID))
                .AsNoTracking()
                .ToListAsync();

            return View(allTickets);
        }

        //票區位子監控
        public async Task<IActionResult> ActiveMonitorList(string? keyword)
        {
            try
            {
                var activeProgrammes = await _user.GetProgrammesALL(keyword);
                return View(activeProgrammes);
            }
            catch (Exception ex)
            {
                // 🚩 這裡會捕捉到真正的錯誤訊息
                return Content($"崩潰原因: {ex.Message} \n 堆疊追蹤: {ex.StackTrace}");
            }
        }
    }


}
