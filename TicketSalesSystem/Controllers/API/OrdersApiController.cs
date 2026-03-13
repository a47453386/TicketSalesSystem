using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.Service.Orders;
using TicketSalesSystem.Service.Queue;
using TicketSalesSystem.Service.Seats;
using TicketSalesSystem.Service.User;

namespace TicketSalesSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "MemberScheme")]
    public class OrdersApiController : ControllerBase
    {
        private readonly TicketsContext _context;
        private readonly BookingService _bookingService;
        private readonly IUserAccessorService _userAccessorService;
        private readonly IUser _user;
        private readonly IOrderService _orderService;


        public OrdersApiController(TicketsContext context, BookingService bookingService,
            IUserAccessorService userAccessorService, IUser user, IOrderService orderService)
        {
            _context = context;
            _bookingService = bookingService;
            _userAccessorService = userAccessorService;
            _user = user;
            _orderService = orderService;
        }

        //訂單清單
        [HttpGet("OrdersIndex")]
        public async Task<IActionResult> GetOrdersIndex()
        {

            // 1. 取得登入者 ID
            var memberID = _userAccessorService.GetMemberId();
            if (memberID == null) return Unauthorized(new { success = false, message = "請先登入" });


            // 2. 直接調用你剛抽離好的 Service
            var orders = await _user.GetUserOrdersAsync(memberID);
            if (orders == null || !orders.Any())
            {
                return NotFound(new { message = "目前沒有訂單" });
            }


            return Ok(orders); // 回傳 JSON 給 Android
        }



        //訂單詳情
        [HttpGet("OrdersDetail/{id}")]
        public async Task<IActionResult> GetOrderDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("訂單編號不可為空");
            }

            // 2. 直接調用你剛抽離好的 Service
            var orders = await _user.GetUserOrderDetailAsync(id);

            if (orders == null)
            {
                return NotFound(new { message = "找不到該筆訂單" });
            }

            return Ok(orders); // 回傳 JSON 給 Android
        }
    }
}
