using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TicketSalesSystem.Helpers;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.Seats;
using TicketSalesSystem.Service.Validation.NewFolder;
using TicketSalesSystem.ViewModel;

namespace TicketSalesSystem.Controllers
{
    public class SeatsController : Controller

    {
        private readonly ISeatService _seatService;
        private readonly IBookingValidationService _bookingValidation;

        public SeatsController(ISeatService seatService, IBookingValidationService bookingValidation)
        {
            _seatService = seatService;
            _bookingValidation = bookingValidation;
        }

        //使用者訂購頁面

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
                string memberId = "72efc442-e093-4cbb-af04-f3903057871b";
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

    }
}
