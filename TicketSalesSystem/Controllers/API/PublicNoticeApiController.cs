using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class PublicNoticeApiController : ControllerBase
    {
        private readonly TicketsContext _context;
        public PublicNoticeApiController(TicketsContext context)
        {
            _context = context;
        }

        [HttpGet("AllNews")]
        public async Task<IActionResult> GetAllNews()
        {
            var allNews = await _context.PublicNotice
                .Where(p => p.PublicNoticeStatus == true)
                .OrderByDescending(p => p.CreatedTime)
                .ToListAsync();

            return Ok(allNews); // 回傳 JSON 陣列
        }

        //[HttpGet("NewDetails/{id}")] // 🚩 建議將 ID 放入路徑中
        //public async Task<IActionResult> NewDetails(string id)
        //{
        //    if (string.IsNullOrEmpty(id))
        //    {
        //        return BadRequest("ID_REQUIRED");
        //    }

        //    var notice = await _context.PublicNotice
        //        .Where(p => p.PublicNoticeStatus == true)
        //        .FirstOrDefaultAsync(m => m.PublicNoticeID == id);

        //    if (notice == null)
        //    {
        //        return NotFound();
        //    }

        //    // 🚩 改為回傳 Ok(JSON)，Android 才能解析內容
        //    return Ok(notice);
        //}

    }
}
