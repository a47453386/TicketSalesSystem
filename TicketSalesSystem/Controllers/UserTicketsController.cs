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
            // 🚩 修正 Include 路徑：Tickets -> Session -> Programme
            var tickets = await _context.Tickets
                .Include(t => t.TicketsArea)
                // 應從 Tickets 直接連 Session，而不是從 TicketsArea 連
                .Include(t => t.Session)
                    .ThenInclude(s => s.Programme)
                        .ThenInclude(p => p.Place) // 建議連場地也一起抓，列印時才看得到地點
                .Where(t => t.OrderID == orderId && t.TicketsStatusID == "A")
                .ToListAsync();

            if (!tickets.Any())
            {
                // 儲存臨時訊息
                TempData["ErrorMessage"] = "票券尚未開放列印（需於演出前 15 天），或查無可列印票券。";
                // 直接導向到清單頁面
                return RedirectToAction("UserIndex", "UserOrders");
            }

            return View(tickets);
        }
    }
}
