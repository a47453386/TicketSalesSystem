using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.ViewComponents
{
    public class VCVenues : ViewComponent
    {
        private readonly TicketsContext _context;
        public VCVenues(TicketsContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string placeID)
        {
            var venues = await _context.Venue.Where(v => v.PlaceID == placeID).ToListAsync();
            return View(venues);
        }



    }
}
