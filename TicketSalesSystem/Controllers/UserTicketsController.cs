using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "MemberScheme")]
    public class UserTicketsController : Controller
    {
        private readonly TicketsContext _context;

        public UserTicketsController(TicketsContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> PrintTickets(string orderId)
        {
            // 抓取該訂單下已付款且核發查核碼的票券
            var tickets = await _context.Tickets
                .Include(t => t.TicketsArea)
                .ThenInclude(s => s.Session)
                .ThenInclude(a => a.Programme)
                .Where(t => t.OrderID == orderId && t.TicketsStatusID == "A")
                .ToListAsync();

            if (!tickets.Any()) return NotFound("找不到可列印的票券");

            return View(tickets); // 傳送票券列表到 View
        }
    }
}
