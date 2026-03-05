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
    [AllowAnonymous]
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

        [HttpGet("api/OrdersIndex")]
        public async Task<IActionResult> GetOrdersIndex()
        {

            //// 1. 取得登入者 ID
            //var memberID = _userAccessorService.GetMemberId();
            //if (memberID == null) return Unauthorized(new { success = false, message = "請先登入" });


            var memberID = "a8e36451-c3fb-44ba-a05e-602ca0760166";

            // 2. 直接調用你剛抽離好的 Service
            var orders = await _user.GetUserOrdersAsync(memberID);

            return Ok(orders); // 回傳 JSON 給 Android
        }

    }
}
