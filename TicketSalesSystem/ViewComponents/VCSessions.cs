using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.ViewComponents
{
    public class VCSessions: ViewComponent
    {
        private readonly TicketsContext _context;
        public VCSessions(TicketsContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string programmeID)
        {
            var sessions = await _context.Session.Where(v => v.ProgrammeID == programmeID).ToListAsync();
            return View(sessions);
        }
    }
}
