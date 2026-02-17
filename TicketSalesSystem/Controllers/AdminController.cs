using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.ID;
using TicketSalesSystem.Service.Images;

namespace TicketSalesSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly TicketsContext _context;

        public AdminController(TicketsContext context)
        {
            _context = context;
            
        }
        public async Task<IActionResult> Dashboard()
        {
            // 🚩 修正：銷售額 = 已付款訂單 -> 所有票券 -> 票區單價加總
            ViewBag.TotalSales = await _context.Order
                .Where(o => o.PaymentStatus == true)
                .SelectMany(o => o.Tickets) // 展開訂單下的所有票券
                .SumAsync(t => t.TicketsArea.Price); // 加總票區設定的金額

            // 🚩 統計總票數 (已售出)
            ViewBag.TotalTicketsSold = await _context.Tickets
                .CountAsync(t => t.Order.PaymentStatus == true);

            ViewBag.OrderCount = await _context.Order.CountAsync();
            ViewBag.MemberCount = await _context.Member.CountAsync();

            // 取得最近 5 筆訂單，並包含會員與票券資訊
            var latestOrders = await _context.Order
                .Include(o => o.Member)
                .Include(o => o.Tickets) // 為了在首頁顯示這筆訂單買了幾張
                .OrderByDescending(o => o.OrderCreatedTime)
                .Take(5)
                .ToListAsync();

            return View(latestOrders);
        }
    }
}
