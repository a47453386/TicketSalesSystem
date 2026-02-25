using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.ID;

namespace TicketSalesSystem.Controllers
{
   
    public class UserFAQsController : Controller
    {
        private readonly TicketsContext _context;

        public UserFAQsController(TicketsContext context, IIDService idService)
        {
            _context = context;
        }

        public async Task<IActionResult> UserIndex()
        {
            var faqs = await _context.FAQ
                .Include(f => f.FAQPublishStatus)
                .Include(f => f.FAQType)
                .Where(f => f.FAQPublishStatusID == "Y") // 只顯示已發布的 FAQ                
                .ToListAsync();
            return View(faqs);
        }


    }
}
