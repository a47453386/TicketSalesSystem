using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TicketSalesSystem.Helpers;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service;
using TicketSalesSystem.ViewModel;

namespace TicketSalesSystem.Controllers
{
    public class SeatsController : Controller

    {
        private readonly ISeatService _context;

        public SeatsController(ISeatService seatService)
        {
            _context = seatService;
        }

        //使用者訂購頁面

        //選擇場次(顯示)
        public async Task<IActionResult> IndexTest(string id)
        {
            var session = await _context.GetSessionsAsync();
            ViewBag.Sessions = session;
            return View();
        }

        //AJAX抓取票區(取得)
        [HttpGet]
        public async Task<IActionResult> GetAreasBySession(string sessionId)
        {
            var areas = await _context.GetAreasBySession(sessionId);

            return Json(areas);
        }



        [HttpPost]
        public async Task<IActionResult> ConfirmBooking([FromBody] VMBookingRequest request)
        {
            try
            {
                string memberId = "19f0cbe2-88b2-4d73-b285-93dad697a1b8";
                var response = await _context.CreateOrderAndTicketsAsync(request, memberId);
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
