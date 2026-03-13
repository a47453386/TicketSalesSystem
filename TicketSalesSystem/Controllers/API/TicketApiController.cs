using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel.Ticket;

namespace TicketSalesSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    [Authorize(AuthenticationSchemes = "MemberScheme")]
    public class TicketApiController : ControllerBase
    {
        private readonly TicketsContext _context;

        public TicketApiController(TicketsContext context)
        {
            _context = context;
        }
        // 核銷票券 API
        [HttpPost("Verify")]
        public async Task<IActionResult> VerifyTicket([FromBody] VerifyRequest request)
        {
            if (string.IsNullOrEmpty(request.CheckInCode))
            {
                return BadRequest(new { Success = false, Message = "無效的核銷碼" });
            }

            // 1. 尋找票券
            var ticket = await _context.Tickets
                .Include(t => t.Session)
                .FirstOrDefaultAsync(t => t.CheckInCode == request.CheckInCode);

            if (ticket == null)
            {
                return NotFound(new { Success = false, Message = "找不到此票券，請確認代碼是否正確" });
            }

            // 2. 判斷狀態
            //檢查是否已經核銷過 (狀態 Y)
            if (ticket.TicketsStatusID == "Y")
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"此票券已於 {ticket.ScannedTime:yyyy/MM/dd HH:mm:ss} 核銷過，請勿重複使用！"
                });
            }

            //檢查票券是否為有效狀態 (狀態 A)，這樣可以一次擋掉「退票」、「尚未付款」或「取消」等非 A 的票
            if (ticket.TicketsStatusID != "A")
            {
                return BadRequest(new { Success = false, Message = "此票券目前狀態無效（可能已退票或尚未付款成功）" });
            }

            // 5. 判斷場次日期 (確保 Session 不為空)
            if (ticket.Session == null)
            {
                return BadRequest(new { Success = false, Message = "系統錯誤：此票券無關聯的場次資訊" });
            }

            if (ticket.Session.StartTime.Date != DateTime.Today)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"此票券非今日場次（場次日期：{ticket.Session.StartTime:yyyy/MM/dd}），請重新確認"
                });
            }

            // 3. 執行核銷動作
            ticket.TicketsStatusID = "Y"; // 設為已使用
            ticket.ScannedTime = DateTime.Now; // 記錄核銷時間

            try
            {
                await _context.SaveChangesAsync();

                // 可以回傳一些票券資訊給 App 顯示，增加使用者體驗
                return Ok(new
                {
                    Success = true,
                    Message = "核銷成功！歡迎入場",
                    Data = new
                    {
                        TicketID = ticket.TicketsID,
                        ScannedTime = ticket.ScannedTime?.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Success = false, Message = "伺服器更新失敗，請重新嘗試" });
            }
        }
    }

    
}

