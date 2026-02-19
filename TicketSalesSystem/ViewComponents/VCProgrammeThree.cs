using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.ViewComponents
{
    public class VCProgrammeThree: ViewComponent
    {
        private readonly TicketsContext _context;
        public VCProgrammeThree(TicketsContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var programmes = await _context.Programme
                .Include(p => p.Session)
                .Include(p => p.Place)
                .Where(p => p.ProgrammeStatusID=="O")
                .OrderByDescending(p => p.CreatedTime)
                .Take(3)
                .ToListAsync();
            return View(programmes);
        }



    }
}
