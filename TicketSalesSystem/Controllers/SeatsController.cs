using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.Controllers
{
    public class SeatsController : Controller

    {
        private readonly TicketsContext _context;

        public SeatsController(TicketsContext context)
        {
            _context = context;
        }
       
        
        public async Task<IActionResult> Index(string venueID,string sessionID)
        {
            if (venueID == null)
            { 
                return NotFound("找不到該區域配置"); 
            }
            if (sessionID == null)
            {
                return NotFound("找不到該場次");
            }
            var Venue = await _context.Venue.FirstOrDefaultAsync(v => v.VenueID == venueID);
            var Session = await _context.Session.FirstOrDefaultAsync(s => s.SessionID == sessionID);

            var seatStatus = await _context.TicketsArea
                .Where(t => t.VenueID == venueID && t.SessionID == sessionID)
                .ToListAsync();

            List<Seats> seatList = new List<Seats>();

            for(int r=1;r<=Venue.RowCount;r++)
            {
                for (int s = 1; s <= Venue.SeatCount; s++)
                { 
                    var rowName = r+"排";
                    var seatName = s + "號";

                    // 檢查這個位置是否已經在資料庫的 seatStatus 裡面
                    bool isSold = seatStatus.Any(t => t.TicketsAreaName == $"{rowName}{seatName}");

                    seatList.Add(new Seats
                    {
                        AreaID = Venue.VenueID,
                        rowIndex = r,
                        SeatIndex = s,
                        RowName = rowName,
                        SeatName = seatName,
                        Status = isSold ? "已售" : "可售" 
                    });
                }
            }
            ViewBag.VenueName = Venue.VenueName;


            return View(seatList);
        }
    }
}
