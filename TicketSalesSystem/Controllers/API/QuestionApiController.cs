using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.Service.User;

namespace TicketSalesSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "MemberScheme")]
    public class QuestionApiController : ControllerBase
    {
        private readonly IUser _user;
        private readonly TicketsContext _context;
        private readonly IUserAccessorService _userAccessorService;

        public QuestionApiController(IUser user, TicketsContext context, IUserAccessorService userAccessor)
        {
            _user = user;
            _context = context;
            _userAccessorService = userAccessor;
        }
        //問題清單
        [HttpGet("GetMyQuestions")]
        public async Task<IActionResult> GetMyQuestions()
        {
            var memberID = _userAccessorService.GetMemberId();
            if (memberID == null) return Unauthorized();

            var questions = await _user.GetMemberQuestionsAsync(memberID);
            if (questions == null || !questions.Any())
            {
                // 這裡回傳 404，Android 就會立刻跳到黃色文字畫面
                return NotFound(new { message = "目前尚未諮詢" });
            }

            // 🚩 投影成 Android 端能解析的 JSON 格式
            var result = questions.Select(q => new {
                QuestionID = q.QuestionID,
                QuestionTitle = q.QuestionTitle,
                QuestionDescription = q.QuestionDescription,
                CreatedTime = q.CreatedTime.ToString("yyyy-MM-dd HH:mm"),
                QuestionTypeName = q.QuestionType?.QuestionTypeName, // 補上種類名稱
                                                                     // 抓取最後一筆回覆的狀態，若無則預設為 "P" (處理中)
                ReplyStatusID = q.Reply?.OrderByDescending(r => r.CreatedTime).FirstOrDefault()?.ReplyStatusID ?? "P",
                HasUpload = !string.IsNullOrEmpty(q.UploadFile) // 標記是否有附件
            });

            return Ok(result);
        }

        //問題詳細資料
        [HttpGet("GetQuestionsDetail/{id}")]
        public async Task<IActionResult> GetQuestionDetail(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var memberID = _userAccessorService.GetMemberId();
            if (memberID == null) return Unauthorized();


            // 直接獲取 DTO
            var questionDto = await _user.GetQuestionDetailForUserAsync(id, memberID);

            if (questionDto == null) return NotFound();

            // 🚩 API 不再需要 SelectList，讓前端根據 QuestionTypeName 或邏輯自行判斷即可
            return Ok(questionDto);

        }
    }
}
