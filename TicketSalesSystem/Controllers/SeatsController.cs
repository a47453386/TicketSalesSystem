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
        private readonly TicketsContext _Context;

        public SeatsController(ISeatService seatService, IBookingValidationService bookingValidation,TicketsContext context)
        {
            _seatService = seatService;
            _bookingValidation = bookingValidation;
            _Context = context;
        }

        //使用者訂購頁面
        public async Task<IActionResult> Index(string id) 
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            //取得場次資訊
            var session =await _Context.Session
                .Include(s => s.Programme)
                .Include(s => s.TicketsArea)
                .FirstOrDefaultAsync(s => s.SessionID == id);
            if (session == null) return NotFound();

            //檢查是否狀態
            if (session.Programme.ProgrammeStatusID !="O")
            {
                ViewData["Message"] = $"本活動目前狀態為：{session.Programme.ProgrammeStatus.ProgrammeStatusName}，暫不開放購票。";
                return RedirectToAction("Index","Programmes");
            }

            //檢查是否在售票時間內
            if(DateTime.Now<session.SaleStartTime||DateTime.Now>session.SaleEndTime)
            {
                TempData["Message"] = "本時段尚未開放購票";
                return RedirectToAction("Index", "Programmes");
            }
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
