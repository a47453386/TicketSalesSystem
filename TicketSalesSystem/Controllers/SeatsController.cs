using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Helpers;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel;

namespace TicketSalesSystem.Controllers
{
    public class SeatsController : Controller

    {
        private readonly TicketsContext _context;

        public SeatsController(TicketsContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string venueID, string sessionID, string areaID)
        {
            // 1. 從資料庫取得場館資訊 (座位數量)
            var venue = await _context.Venue.FirstOrDefaultAsync(v => v.VenueID == venueID);
            if (venue == null)
            {
                return NotFound("找不到場地配置");
            }

            //2. 取得該場次、該票區「已售出」的 Tickets 列表
            var soldTickets = await _context.Tickets
                .Where(t => t.SessionID == sessionID && t.TicketsAreaID == areaID)
                .ToListAsync();

            // 3.呼叫 Helper 產生完整的座位清單 (Model)
            var seatList = SeatHelper.GenerateSeatLayout(venue.RowCount, venue.SeatCount, soldTickets);


            ViewBag.VenueID = venueID;
            ViewBag.SessionID = sessionID;
            ViewBag.AreaID = areaID;

            // 4. 將清單傳給 Index.cshtml
            return View(seatList);
        }

        [HttpPost]
        public async Task<IActionResult> AutoAssign([FromBody] AssignRequest request)
        {
            // 1. 取得場館資訊
            var venue = await _context.Venue.FindAsync(request.VenueID);
            if (venue == null)
            {
                return Json(new { success = false, message = "找不到場地配置" });
            }
            //2. 取得該場次已售出的票
            var soldTickets = await _context.Tickets.Where(t => t.SessionID == request.SessionID && t.TicketsAreaID == request.AreaID).ToListAsync();

            // 2. 產出當前所有座位狀態
            var allSeats = SeatHelper.GenerateSeatLayout(venue.RowCount, venue.SeatCount, soldTickets);

            // 3. 找出連續座位
            var selectedIDs = SeatHelper.FindBestSeats(allSeats, request.Count);

            if (!selectedIDs.Any())
            {
                return Json(new { success = false, message = "無法找到足夠的連續座位" });
            }


            return Json(new { success = true, seats = selectedIDs });

        }


        [HttpPost]
        public async Task<IActionResult> ConfirmBooking([FromBody] BookingRequest request)
        {
            var area = await _context.TicketsArea.FindAsync(request.AreaID);
            if (area == null)
            {
                return Json(new { success = false, message = "找不到票區資訊" });
            }

            decimal price = area.Price;
            decimal totalAmount = price * request.Seats.Count;

            var newOrederID = _context.Database.SqlQueryRaw<string>("SELECT dbo.funGetOrderID() ").AsEnumerable().FirstOrDefault();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. 建立訂單
                var order = new Order
                {
                    OrderID = newOrederID,
                    MemberID = request.MemberID,
                    SessionID = request.SessionID,
                    PaymentMethodID = request.PaymentMethodID,
                    OrderCreatedTime = DateTime.Now,
                };

                _context.Order.Add(order);
                await _context.SaveChangesAsync();
                // 2. 建立票券
                foreach (var seatID in request.Seats)
                {
                    var ticket = new Tickets
                    {
                        //TicketID = _context.Database.SqlQueryRaw<string>("SELECT dbo.funGetTicketID() ").AsEnumerable().FirstOrDefault(),
                        //SessionID = request.SessionID,
                        //TicketsAreaID = request.AreaID,
                        //SeatID = seatID,
                        //OrderID = newOrederID,
                        //Price = area.Price
                    };
                    _context.Tickets.Add(ticket);
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, message = "訂票成功", orderID = newOrederID });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "訂票失敗: " + ex.Message });
            }
        }
    }
}
