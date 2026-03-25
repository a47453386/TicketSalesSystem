    using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using TicketSalesSystem.DTOs;
using TicketSalesSystem.Helpers;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.Service.Orders;
using TicketSalesSystem.Service.Queue;
using TicketSalesSystem.Service.Seats;
using TicketSalesSystem.Service.Validation.NewFolder;
using TicketSalesSystem.ViewModel;
using TicketSalesSystem.ViewModel.Booking;

namespace TicketSalesSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "MemberScheme")]
    public class SeatsController : Controller

    {
        private readonly ISeatService _seatService;
        private readonly IBookingValidationService _bookingValidation;
        private readonly TicketsContext _context;
        private readonly IOrderService _orderService;
        private readonly IMemoryCache _memoryCache;
        private readonly IQueueService _queueService;
        private readonly IUserAccessorService _userAccessorService;
        private readonly BookingService _bookingService;

        public SeatsController(ISeatService seatService, IBookingValidationService bookingValidation, 
            TicketsContext context, IOrderService orderService, IMemoryCache memoryCache,
            IQueueService queueService,IUserAccessorService userAccessorService, BookingService bookingService)
        {
            _seatService = seatService;
            _bookingValidation = bookingValidation;
            _context = context;
            _orderService = orderService;
            _memoryCache = memoryCache;
            _queueService = queueService;
            _bookingService = bookingService;
            _userAccessorService = userAccessorService;
        }

        // 【入口門衛】--當使用者點擊「立即購票」時，這是他們看到的第一站。
        [HttpGet]
        public IActionResult WaitingRoom(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            ViewBag.IsPhoneVerified = _userAccessorService.IsPhoneVerified();

            // 如果他已經驗證過且拿到通行證了，就不用再排隊，直接去 Index
            var token = HttpContext.Session.GetString("BookingToken");
            if (token == "Verified_" + id)
            {
                return RedirectToAction("Index", "Seats", new { id = id });
            }

            return View("WaitingRoom", (object)id); // 導向 WaitingRoom.cshtml
        }

        //【身分驗證與快速通行】--這是使用者在排隊頁面按下「確認送出」時觸發的邏輯。
        [HttpPost]
        public IActionResult VerifyGateCaptcha(string id, string captchaInput, string sessionCaptcha)
        {
            if (!_userAccessorService.IsPhoneVerified())
            {
                return Json(new { success = false, phoneNotVerified = true });
            }


            // 🚩 2. 驗證碼比對
            if (string.IsNullOrEmpty(captchaInput) || !captchaInput.Equals(sessionCaptcha, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "驗證碼錯誤，請重新產生" });
            }

            // --- 到這裡代表：驗證碼正確 且 手機已驗證 ---

            // 🚩 3. 發放通行證 (Session 紀錄)
            // 防止使用者直接輸入網址進入 Index 頁面
            HttpContext.Session.SetString("GatePassed_" + id, "true");
            HttpContext.Session.SetString("BookingToken", "Verified_" + id);

            // 🚩 4. 流量削峰 (排隊邏輯)
            int limit = 5; // Demo 用，實際可調高
            int currentActive = _memoryCache.Get<int>("ActiveUserCount");

            if (currentActive < limit)
            {
                // 進入人數未滿，直接放行
                _memoryCache.Set("ActiveUserCount", currentActive + 1, TimeSpan.FromMinutes(10));
                return Json(new { success = true, isQueuing = false });
            }

            // 進入人數已滿，通知前端啟動輪詢 (startPolling)
            return Json(new { success = true, isQueuing = true });
        }


        // 【排隊監測 API】--這是給前端「轉圈圈頁面」用 JavaScript 定時（例如每 2 秒）呼叫的。
        [HttpGet]
        public IActionResult CheckQueueStatus(string id)
        {
            // 先檢查他有沒有「過門」
            if (HttpContext.Session.GetString("HumanVerified_" + id) != "true")
            {
                return Json(new { canEnter = false, needCaptcha = true });
            }

            int limit = 5;
            int currentActive = _memoryCache.Get<int>("ActiveUserCount");

            if (currentActive < limit)
            {
                _memoryCache.Set("ActiveUserCount", currentActive + 1, TimeSpan.FromMinutes(10));
                HttpContext.Session.SetString("BookingToken", "Verified_" + id);

                string nextUrl = $"/Seats/Index/{id}";

                return Json(new
                {
                    canEnter = true,
                    redirectUrl = nextUrl
                });
            }

            return Json(new { canEnter = false });
        }









        //使用者訂購頁面
        public async Task<IActionResult> Index(string id)
        {

            if (string.IsNullOrEmpty(id)) return NotFound();

            //取得目前會員 ID (這裡先用你代碼中的測試 ID)
            var memberID = _userAccessorService.GetMemberId();

            if (memberID == null)
            {
                return RedirectToAction("MemberLogin", "Login");
            }
            var member = await _context.Member.FirstOrDefaultAsync(m => m.MemberID == memberID);
           
            // 檢查是否有通行證，防止直接輸入網址跳過排隊
            var gatePassed = HttpContext.Session.GetString("GatePassed_" + id);
            var bookingToken = HttpContext.Session.GetString("BookingToken");
            if (bookingToken != "Verified_" + id)
            {
                return RedirectToAction("WaitingRoom", new { id = id });
            }

            ViewBag.IsVerified = (gatePassed == "true");

            //取得場次資訊
            var session = await _context.Session
                .Include(s => s.Programme)
                .ThenInclude(p => p.ProgrammeStatus)
                .Include(s => s.TicketsArea)
                .FirstOrDefaultAsync(s => s.SessionID == id);
            if (session == null) return NotFound();

            //檢查是否狀態
            if (session.Programme.ProgrammeStatusID != "O")
            {
                ViewData["Message"] = $"本活動目前狀態為：{session.Programme.ProgrammeStatus.ProgrammeStatusName}，暫不開放購票。";
                return RedirectToAction("Index", "Programmes");
            }



            //統計該會員在「該活動」下所有場次已持有的有效票數
            // 包含：已付款 (Y)、待付款 (P)
            var ticketActiveStatuses = new[] { "P", "Y" ,"A","S"};
            int purchasedCount = await _context.Tickets
                .Where(t => t.Order.MemberID == memberID &&
                            t.Session.ProgrammeID == session.ProgrammeID &&
                            ticketActiveStatuses.Contains(t.TicketsStatusID))
                .CountAsync();

            //計算剩餘可購張數
            int limit = session.Programme.LimitPerOrder ?? 4; // 預設 4
            int remainingQuota = limit - purchasedCount;

            //將結果傳給 View
            ViewBag.RemainingQuota = Math.Max(0, remainingQuota);

            return View(session);

        }





        [HttpPost]
        public async Task<IActionResult> ConfirmBooking([FromBody] VMBookingRequest request)
        {
            
            var memberID = _userAccessorService.GetMemberId();

            if (memberID == null)
            {
                return RedirectToAction("MemberLogin", "Login");
            }

            var memberExists = await _context.Member.AnyAsync(m => m.MemberID == memberID);
            if (!memberExists)
            {
                return Json(new { success = false, message = "會員不存在" });
            }

            var result = await _bookingService.ConfirmBookingAsync(request, memberID);

            if (result.ShouldRedirectToLogin) return RedirectToAction("MemberLogin", "Login");

            // 網頁端通常回傳 JSON 給前台的 AJAX
            return Json(result.Success ? result.Data : new { success = false, message = result.Message });


        }







        //「看」訂單內容並準備付錢
        public async Task<IActionResult> Payment(string id)
        {
            if (id == null) return NotFound();

            var vm = await _orderService.GetPaymentDetailsAsync(id);

            if (vm == null) return NotFound();

            ViewData["PaymentMethods"] = await _context.PaymentMethod.ToListAsync();

            return View(vm);
        }


        //是讓你「正式結帳」**並領取票券憑證
        [HttpPost]
        public async Task<IActionResult> Payment([FromBody] VMPaymentRequest request)
        {
            // 🚩 防禦性檢查：防止前端傳入空值
            if (string.IsNullOrEmpty(request.OrderID) || string.IsNullOrEmpty(request.PaymentMethodID))
            {
                return Json(new { success = false, message = "傳輸資料不完整" });
            }
            try
            {
                // 2. 直接交給專業的來（呼叫 Service）
                // 內部會自動處理：是否存在、是否超時(N)、是否已付款(Y)、原子更新、產票
                var result = await _orderService.ProcessPaymentAsync(request);

                if (result.Success)
                {
                    // 3. 執行網頁版特有的收尾動作
                    _queueService.ReleaseQueueSlot();
                    HttpContext.Session.Remove("BookingToken");

                    return Json(new { success = true, message = result.Message });
                }

                // 4. 如果失敗，直接回傳 Service 給的錯誤訊息（例如：已逾時）
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                
                var detail = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = "系統更新異常：" + detail });
            }
        }

    }

}
