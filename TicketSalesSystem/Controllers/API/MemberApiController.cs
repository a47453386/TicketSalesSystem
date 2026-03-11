using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.User;
using TicketSalesSystem.ViewModel.Member;

namespace TicketSalesSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "MemberScheme")]
    public class MemberApiController : ControllerBase
    {
        private readonly IUser _userService;
        private readonly TicketsContext _context;
        public MemberApiController(IUser userService, TicketsContext context)
        {
            _userService = userService;
            _context = context;
        }

        //會員資料
        [HttpGet("MembersDetails/{id}")]
        public async Task<IActionResult> GetMemberDetails(string id)
        {

            var member = await _context.Member
             .Include(m => m.AccountStatus)
             .FirstOrDefaultAsync(m => m.MemberID == id);

            if (member == null) return NotFound();

            var memberLogin = await _context.MemberLogin.FindAsync(id);

            var data = new
            {
                MemberID = id,
                Name = member.Name,
                Address = member.Address,
                Birthday = member.Birthday.ToString("yyyy-MM-dd"), // 格式化日期
                Tel = member.Tel,
                Gender = member.Gender,
                NationalID = member.NationalID,
                Email = member.Email,
                IsPhoneVerified = member.IsPhoneVerified,
                StatusName = member.AccountStatus?.AccountStatusName ?? "正常"
            };
            return Ok(data);
        }

        //會員資料編輯
        [HttpGet("GetProfile/{id}")]
        public async Task<IActionResult> GetProfile(string id)
        {
            var member = await _context.Member.FindAsync(id);
            if (member == null) return NotFound();

            // 🚩 投影成 Android 需要的格式
            var result = new
            {
                member.MemberID,
                member.Name,
                member.Birthday,
                member.Tel,
                member.Email,
                member.Address,
                member.NationalID,
                member.Gender,
                Account = (await _context.MemberLogin.FindAsync(id))?.Account
            };
            return Ok(result);
        }        

        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] VMMemberUserEdit vm)
        {
            // API 不使用 ModelState 驗證，直接執行 Service
            var result = await _userService.UpdateMemberProfileAsync(vm);

            if (result.success)
            {
                return Ok(new { message = result.message });
            }
            return BadRequest(new { message = result.message });
        }
    }
}
