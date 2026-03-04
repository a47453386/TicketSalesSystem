using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.Service.Seats;
using TicketSalesSystem.Service.User;
using TicketSalesSystem.ViewModel.Booking;

namespace TicketSalesSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class SeatsApiController : ControllerBase
    {
        private readonly TicketsContext _context; 
        private readonly BookingService _bookingService;
        private readonly IUserAccessorService _userAccessorService;
        private readonly IUser _user;



        public SeatsApiController(TicketsContext context,BookingService bookingService,
            IUserAccessorService userAccessorService, IUser user    )
        {
            _context = context;
            _bookingService = bookingService;
            _userAccessorService = userAccessorService;
            _user = user;
        }

        //API Index
        [HttpGet("apihome")]
        public async Task<IActionResult> GetHomeData()
        {
            // 直接叫 Service 做事，Controller 只負責回傳 Ok
            var data = await _user.GetProgrammesALL();
            return Ok(data);
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> APIConfirm([FromBody] VMBookingRequest request)
        {
            //var memberID = _userAccessorService.GetMemberId();
            var memberID = "a8e36451-c3fb-44ba-a05e-602ca0760166";

            if (string.IsNullOrEmpty(memberID))
                return Unauthorized(new { message = "請先登入" });

            var result = await _bookingService.ConfirmBookingAsync(request, memberID);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result.Data); // 回傳 200 OK 與訂單資料
        }
    }
}
