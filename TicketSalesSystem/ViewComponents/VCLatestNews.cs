using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.ViewComponents
{
    public class VCLatestNews : ViewComponent
    {
        private readonly TicketsContext _context;
        public VCLatestNews(TicketsContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var latestNews =await _context.PublicNotice
                .Where(p => p.PublicNoticeStatus == true)
                .OrderByDescending(n => n.CreatedTime)
                .Take(5)
                .ToListAsync();
            return View(latestNews);

        }
    }
}
