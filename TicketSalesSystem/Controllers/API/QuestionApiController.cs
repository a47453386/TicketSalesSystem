using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.DTOs.Question;
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


        //我要發問
        [HttpPost("QuestionsCreate")]
        [AllowAnonymous] // 避免 Cookie 沒帶好導致 401，改由內部判斷 ID
        public async Task<IActionResult> QuestionsCreate([FromForm] QuestionCreateDto dto, [FromForm] string? MemberID, IFormFile? upload)
        {
            // 1. 取得會員 ID (優先抓 Session，抓不到就抓 Android 傳來的 MemberID)
            var memberID = _userAccessorService.GetMemberId() ?? MemberID;

            if (string.IsNullOrEmpty(memberID))
            {
                return Unauthorized(new { message = "身分驗證失敗，請重新登入" });
            }

            // 2. 建立 Question 物件並手動對齊 (模仿 MVC 的行為)
            var question = new Question
            {
                QuestionTitle = dto.QuestionTitle,
                QuestionDescription = dto.QuestionDescription,
                QuestionTypeID = dto.QuestionTypeID
                // OrderID = dto.OrderID // 如果 Android 端有連動訂單，這裡也要補
            };

            // 3. 移除不需要前端驗證的欄位 (這幾行就是你 MVC 成功的關鍵)
            ModelState.Remove("QuestionID");
            ModelState.Remove("MemberID");
            ModelState.Remove("CreatedTime");

            // 4. 執行邏輯
            if (ModelState.IsValid)
            {
                try
                {
                    // 呼叫與 MVC 相同的 Service 方法
                    bool isSuccess = await _user.CreateQuestionAsync(question, upload, memberID);

                    if (isSuccess)
                    {
                        return Ok(new { success = true, message = "發送成功" });
                    }

                    return StatusCode(500, "資料庫存檔失敗");
                }
                catch (Exception ex)
                {
                    // 如果報錯，噴出真正的錯誤原因方便 Android 除錯
                    return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
                }
            }

            // 如果資料格式不對 (例如少傳主旨)，回傳 400 與錯誤詳情
            return BadRequest(ModelState);
        }

        [HttpGet("GetQuestionTypes")]
        [AllowAnonymous]
        public async Task<IActionResult> GetQuestionTypes()
        {
            
            var types = await _context.QuestionType
                .Select(t => new {
                    // 這裡對應你資料庫實際的欄位名
                    Id = t.QuestionTypeID,
                    Name = t.QuestionTypeName
                })
                .ToListAsync();

            return Ok(types);
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
