using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TicketSalesSystem.Helpers;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.Seats;
using TicketSalesSystem.Service.Validation.NewFolder;
using TicketSalesSystem.ViewModel;
using TicketSalesSystem.ViewModel.Booking;

namespace TicketSalesSystem.Controllers
{
    public class SeatsController : Controller

    {
        private readonly ISeatService _seatService;
        private readonly IBookingValidationService _bookingValidation;
        private readonly TicketsContext _context;

        public SeatsController(ISeatService seatService, IBookingValidationService bookingValidation, TicketsContext context)
        {
            _seatService = seatService;
            _bookingValidation = bookingValidation;
            _context = context;
        }

        //使用者訂購頁面
        public async Task<IActionResult> Index(string id)
        {

            if (string.IsNullOrEmpty(id)) return NotFound();

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

            ////檢查是否在售票時間內
            //if(DateTime.Now<session.SaleStartTime||DateTime.Now>session.SaleEndTime)
            //{
            //    TempData["Message"] = "本時段尚未開放購票";
            //    return RedirectToAction("Index", "Programmes");
            //}
            return View(session);

        }



        //選擇場次(顯示)
        public async Task<IActionResult> IndexTest(string id)
        {
            var session = await _seatService.GetSessionsAsync();
            ViewBag.Session = session;
            return View();
        }

        //AJAX抓取票區(取得)
        [HttpGet]
        public async Task<IActionResult> GetAreasBySession(string sessionId)
        {
            var areas = await _seatService.GetAreasBySession(sessionId);

            return Json(areas);
        }



        [HttpPost]
        public async Task<IActionResult> ConfirmBooking([FromBody] VMBookingRequest request)
        {


            var (isValid, message) = await _bookingValidation.ValidateAllAsync(request);
            if (!isValid)
            {
                return Json(new VMBookingResponse { Success = false, Message = message });
            }

            try
            {
                string memberId = "83542560-1941-48b9-af77-00fc42536fed";
                var response = await _seatService.CreateOrderAndTicketsAsync(request, memberId);
                return Json(response);

            }
            catch (Exception ex)
            {
                // 這樣可以在瀏覽器 alert 看到更詳細的錯誤內容
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : "";
                return BadRequest(new { message = ex.Message + " | " + innerMsg });

            }

        }

        public async Task<IActionResult> Success(string id)
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

        [HttpPost]
        public async Task<IActionResult> CompletePayment([FromBody] VMPaymentRequest request)
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

               if(currentStatus==null) return Json(new { success = false, message = "找不到此訂單。" });

                if (currentStatus == null)
                    return Json(new { success = false, message = "訂單不存在，可能已被系統徹底移除。" });

                if (currentStatus == "N")
                    return Json(new { success = false, message = "訂單已因超時失效，座位已釋出，請重新購票。" });

                // 2. 執行「原子性更新」(Atomic Update)
                // 只有在狀態依然是暫存狀態(例如 'W' 或 'O')時才更新，這能完美避開背景服務的競爭
                var affectedOrders = await _context.Order
                    .Where(o => o.OrderID == request.OrderID && o.OrderStatusID != "N")
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(o => o.OrderStatusID, "Y")// 改為已完成/處理中
                        .SetProperty(o => o.PaymentStatus, true)// 改為已付款
                        .SetProperty(o => o.PaymentMethodID, request.PaymentMethodID));
                Console.WriteLine($"訂單 {request.OrderID} 更新結果：影響了 {affectedOrders} 行");
                if (affectedOrders > 0)
                {
                    await _context.Tickets
                        .Where(t => t.OrderID == request.OrderID)
                        .ExecuteUpdateAsync(s => s.SetProperty(t => t.TicketsStatusID, "S"));

                    return Json(new { success = true });
                }
                
                return Json(new { success = false, message = "付款請求衝突，請重新整理確認訂單狀態。" });
            }
            catch (Exception ex)
            {
                // 🚩 這裡非常關鍵：如果還是有 DELETE 衝突，這行會把真正的錯誤吐出來
                var detail = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = "系統更新異常：" + detail });
            }
        }


    }

}
