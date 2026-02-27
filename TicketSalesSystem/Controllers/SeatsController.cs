    using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
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

        public SeatsController(ISeatService seatService, IBookingValidationService bookingValidation, 
            TicketsContext context, IOrderService orderService, IMemoryCache memoryCache,
            IQueueService queueService,IUserAccessorService userAccessorService)
        {
            _seatService = seatService;
            _bookingValidation = bookingValidation;
            _context = context;
            _orderService = orderService;
            _memoryCache = memoryCache;
            _queueService = queueService;
            _userAccessorService = userAccessorService;
        }

        // 【入口門衛】--當使用者點擊「立即購票」時，這是他們看到的第一站。
        [HttpGet]
        public IActionResult WaitingRoom(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

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
            // 🚩 這裡不再從 _memoryCache 拿，而是直接比對前端傳過來的「正確答案」
            if (captchaInput != sessionCaptcha) return Json(new { success = false, message = "驗證碼錯誤" });

            // 🚩 執行比對
            if (sessionCaptcha.Equals(captchaInput, StringComparison.OrdinalIgnoreCase))
            {
                // 驗證成功：發放通行證
                HttpContext.Session.SetString("GatePassed_" + id, "true");
                HttpContext.Session.SetString("BookingToken", "Verified_" + id);

                // 檢查人數限制 (流量削峰)
                int limit = 5;
                int currentActive = _memoryCache.Get<int>("ActiveUserCount");

                if (currentActive < limit)
                {
                    _memoryCache.Set("ActiveUserCount", currentActive + 1, TimeSpan.FromMinutes(10));
                    return Json(new { success = true, isQueuing = false });
                }

                return Json(new { success = true, isQueuing = true });
            }

            return Json(new { success = false, message = "驗證碼錯誤，請重新輸入" });
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
            // 包含：已付款 (S)、待付款 (P)
            var activeStatuses = new[] { "P", "S" };
            int purchasedCount = await _context.Tickets
                .Where(t => t.Order.MemberID == memberID &&
                            t.Session.ProgrammeID == session.ProgrammeID &&
                            activeStatuses.Contains(t.Order.OrderStatusID))
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
            //先檢查目前是否已有交易，沒有才開啟
            var transaction = _context.Database.CurrentTransaction == null
                              ? await _context.Database.BeginTransactionAsync()
                              : null;

            var memberID = _userAccessorService.GetMemberId();

            if (memberID == null)
            {
                return RedirectToAction("MemberLogin", "Login");
            }

            var member = await _context.Member.AnyAsync(m => m.MemberID == memberID);
            if (!member)
            {
                return Json(new VMBookingResponse { Success = false, Message = "會員不存在" });
            }


            var (isValid, message) = await _bookingValidation.ValidateAllAsync(request, memberID);
            if (!isValid)
            {
                return Json(new VMBookingResponse { Success = false, Message = message });
            }



            try
            {
                //原子扣減
                var rowAffected = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE TicketsArea SET Remaining = Remaining - {0} " +
                    "WHERE TicketsAreaID = {1} AND Remaining >= {2}",
                    request.Count, request.TicketsAreaID, request.Count
                    );

                if (rowAffected == 0)
                {
                    if (transaction != null) await transaction.RollbackAsync();
                    _queueService.ReleaseQueueSlot();
                    return Json(new { success = false, message = "抱歉，票券已售完！" });
                }


                //建立訂單與寫入
                var response = await _seatService.CreateOrderAndTicketsAsync(request, memberID);


                if (response.Success)
                {
                    if (transaction != null) await transaction.CommitAsync();
                    return Json(response);
                }
                else
                {
                    if (transaction != null) await transaction.RollbackAsync();
                    _queueService.ReleaseQueueSlot();
                    return Json(response);
                }

               

            }
            catch (Exception ex)
            {
                if (transaction != null) await transaction.RollbackAsync();
                _queueService.ReleaseQueueSlot();
                var detail = ex.InnerException?.Message ?? ex.Message;
                return Json(new VMBookingResponse { Success = false, Message = "系統異常：" + detail });

            }


        }







        //「看」訂單內容並準備付錢
        public async Task<IActionResult> Payment(string id)
        {
            if (id == null) return NotFound();

            var VMOrder = await _context.Order
                .Include(o => o.Session).ThenInclude(s => s.Programme).ThenInclude(p => p.Place)
                .Include(o => o.Tickets).ThenInclude(t => t.TicketsArea)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (VMOrder == null) return NotFound();
            ViewData["PaymentMethods"] = await _context.PaymentMethod.ToListAsync();
            var expireTime = VMOrder.OrderCreatedTime.AddMinutes(10);
            var remainingSeconds = (int)(expireTime - DateTime.Now).TotalSeconds;

            var vm = new VMBookingResponse
            {
                ProgrammeName = VMOrder.Session.Programme.ProgrammeName,
                StartTime = VMOrder.Session.StartTime.ToString("yyyy-MM-dd HH:mm"),
                PlaceName = VMOrder.Session.Programme.Place.PlaceName,
                Success = true,
                OrderID = VMOrder.OrderID,
                FinalAmount = VMOrder.Tickets.Sum(t => t.TicketsArea.Price),
                Seats = VMOrder.Tickets.Select(t => $"{t.TicketsArea.TicketsAreaName} 區 {t.RowIndex} 排{t.SeatIndex} 號").ToList(),
                RemainingSeconds = remainingSeconds > 0 ? remainingSeconds : 0,
                ExpireTimeText = expireTime.ToString("yyyy-MM-ddTHH:mm:ss")
            };

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
                // 1. 先確認目前訂單的真實狀態（不追蹤，避免干擾）
                var currentStatus = await _context.Order
                    .AsNoTracking()
                    .Where(o => o.OrderID == request.OrderID)
                    .Select(o => o.OrderStatusID)
                    .FirstOrDefaultAsync();



                if (currentStatus == null)
                    return Json(new { success = false, message = "訂單不存在，可能已被系統徹底移除。" });

                if (currentStatus == "N")
                    return Json(new { success = false, message = "訂單已因超時失效，座位已釋出，請重新購票。" });

                var result = await _orderService.ProcessPaymentAsync(request);

                if (result.Success)
                {
                    _queueService.ReleaseQueueSlot();
                    HttpContext.Session.Remove("BookingToken");
                    return Json(new { success = true, message = "付款成功！" });
                }

                return Json(new { success = false, result.Message });
            }
            catch (Exception ex)
            {
                
                var detail = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = "系統更新異常：" + detail });
            }
        }

    }

}
