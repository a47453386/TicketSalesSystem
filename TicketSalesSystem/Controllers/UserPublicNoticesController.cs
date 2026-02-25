using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.IUserAccessor;

namespace TicketSalesSystem.Controllers
{
    
    public class UserPublicNoticesController : Controller
    {
        private readonly TicketsContext _context;
        
        public UserPublicNoticesController(TicketsContext context)
        {
            _context = context;
            
        }

        public IActionResult UserIndex()
        {
            var publicNotices = _context.PublicNotice
                .Where(p => p.PublicNoticeStatus == true)
                .ToList();
            return View(publicNotices);
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publicNotices = await _context.PublicNotice
               .Where(p => p.PublicNoticeStatus == true)
               .FirstOrDefaultAsync(m => m.PublicNoticeID == id);

            if (publicNotices == null)
            {
                return NotFound();
            }

            return View(publicNotices);
        }

    }
}
