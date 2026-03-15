using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.Service.Orders;
using TicketSalesSystem.Service.Seats;
using TicketSalesSystem.Service.User;
using TicketSalesSystem.ViewModel.Booking;
using TicketSalesSystem.ViewModel.Programme.ProgrammeAdminDetail;

namespace TicketSalesSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "MemberScheme")]
    public class SeatsApiController : ControllerBase
    {
        private readonly TicketsContext _context; 
        private readonly BookingService _bookingService;
        private readonly IUserAccessorService _userAccessorService;
        private readonly IUser _user;
        private readonly IOrderService _orderService;


        public SeatsApiController(TicketsContext context,BookingService bookingService,
            IUserAccessorService userAccessorService, IUser user , IOrderService orderService)
        {
            _context = context;
            _bookingService = bookingService;
            _userAccessorService = userAccessorService;
            _user = user;
            _orderService = orderService;
        }

        


        

        // 確認訂單
        [HttpPost("confirm")]
        public async Task<IActionResult> APIConfirm([FromBody] VMBookingRequest request)
        {
            var memberID = _userAccessorService.GetMemberId();

            if (string.IsNullOrEmpty(memberID))
                return Unauthorized(new { message = "請先登入" });

            var result = await _bookingService.ConfirmBookingAsync(request, memberID);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result.Data); // 回傳 200 OK 與訂單資料
        }

        // 付款並產碼
        [HttpPost("payment")]
        public async Task<IActionResult> PostPayment([FromBody] VMPaymentRequest request)
        {
            // 1. 驗證輸入資料
            if (request == null || string.IsNullOrEmpty(request.OrderID))
            {
                return BadRequest(new { success = false, message = "請提供正確的訂單編號" });
            }

            try
            {
                // 2. 🚩 呼叫剛才寫好的 Service 方法
                // 這個方法裡面包含了 Transaction、訂單更新、票券狀態變更(P -> S)以及產碼
                var (success, message) = await _orderService.ProcessPaymentAsync(request);

                if (!success)
                {
                    // 如果失敗（例如訂單已逾期、被刪除等），回傳錯誤訊息
                    return BadRequest(new { success = false, message = message });
                }

                // 3. 成功後回傳 OK
                return Ok(new { success = true, message = "付款成功，票券已核發！" });
            }
            catch (Exception ex)
            {
                var error = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { success = false, message = "系統執行異常：" + error });
            }
        }

        































        // 🚩 測試用：輸入 SessionID 看能不能找到票區
        [HttpGet("test-session/{sessionId}")]
        public async Task<ActionResult> GetSessionDetailTest(string sessionId)
        {
            // 使用 Include 抓取場次及其關聯的票區
            var session = await _context.Session
                .Include(s => s.TicketsArea) // 🚩 關鍵：載入該場次的所有票區
                .FirstOrDefaultAsync(s => s.SessionID == sessionId);

            if (session == null) return NotFound("找不到該場次");

            // 對應到你的 ViewModel
            var result = new VMSessionDetail
            {
                SessionID = session.SessionID,
                StartTime = session.StartTime,
                TicketsAreas = session.TicketsArea.Select(a => new VMAreaDetail
                {
                    TicketsAreaID = a.TicketsAreaID,
                    TicketsAreaName = a.TicketsAreaName,
                    Price = a.Price,
                    Capacity = a.Capacity,
                    Remaining = a.Capacity
                }).ToList()
            };

            return Ok(result);
        }

        // 🚩 測試用：在你的 ProgrammeController 加入這個測試方法
        [HttpGet("test-area/{sessionId}")]
        public async Task<ActionResult> TestAreaBySession(string sessionId)
        {
            // 1. 直接查詢場次，並載入票區
            var session = await _context.Session
                .Include(s => s.TicketsArea) // 關鍵：一定要 Include
                .FirstOrDefaultAsync(s => s.SessionID == sessionId);

            if (session == null) return NotFound("找不到場次");

            // 2. 轉換為你的 ViewModel 結構
            var result = new
            {
                sessionID = session.SessionID,
                startTime = session.StartTime,
                // 確認這裡是否有資料
                ticketsAreas = session.TicketsArea.Select(a => new {
                    ticketsAreaID = a.TicketsAreaID,
                    ticketsAreaName = a.TicketsAreaName,
                    price = a.Price,
                    remaining = a.Capacity 
                }).ToList()
            };

            return Ok(result);
        }
    }
}
