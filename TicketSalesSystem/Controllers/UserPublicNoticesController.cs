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

        public async Task<IActionResult> UserIndex()
        {
            var now = DateTime.Now;
            var publicNotices = await _context.PublicNotice
                .Where(p => p.PublicNoticeStatus == true && p.PublishTime <= now && (p.RemovalTime == null || p.RemovalTime >= now))
                .OrderByDescending(p => p.PublishTime)
                .ToListAsync();
            return View(publicNotices);
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var now = DateTime.Now;

           
            var publicNotice = await _context.PublicNotice
                .Where(p => p.PublicNoticeStatus == true   // 必須是啟用狀態
                         && p.PublishTime <= now          // 必須已過上架時間
                         &&(p.RemovalTime == null || p.RemovalTime >= now))     
                .FirstOrDefaultAsync(m => m.PublicNoticeID == id);

            if (publicNotice == null)
            {
                // 如果條件不符（例如公告已過期），直接回傳 404
                return NotFound();
            }

            return View(publicNotice);
        }

    }
}
