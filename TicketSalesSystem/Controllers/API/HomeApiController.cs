using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.User;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TicketSalesSystem.Controllers.API
{
    [Route("api/HomeApi")]
    [ApiController]
    [AllowAnonymous]
    public class HomeApiController : ControllerBase
    {
        private readonly IUser _user;
        private readonly TicketsContext _context;
        public HomeApiController(IUser user, TicketsContext context)
        {
            _user = user;
            _context = context;
        }

        //活動清單
        [HttpGet("Home")]
        public async Task<IActionResult> GetHomeData()
        {
            
            var data = await _user.GetProgrammesALL();
            return Ok(data);
        }
        //最新公告5筆
        [HttpGet("FiveNews")]
        public async Task<IActionResult> GetFiveNews()
        {
            var latestNews = await _user.GetLatestFiveNoticesAsync();
            return Ok(latestNews);

        }

        //常見問題
        [HttpGet("FAQs")]
        public async Task<IActionResult> GetFAQs()
        {
            // 1. 呼叫原本的方法，不影響 MVC 邏輯
            var faqs = await _user.GetFAQsAsync();

            // 2. 🚩 在這裡進行投影，只選取 Android 需要的欄位
            // 這會切斷循環引用，因為我們沒有選取 FAQPublishStatus 物件本身
            var result = faqs.Select(f => new {
                faqTitle = f.FAQTitle,
                faqDescription = f.FAQDescription,
                faqTypeName = f.FAQType?.FAQTypeName
            }).ToList();
            return Ok(result);
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
                MemberID = "a8e36451-c3fb-44ba-a05e-602ca0760166",
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


        

        //我要發問
        [HttpPost("QuestionsCreate")]
        public async Task<IActionResult> QuestionsCreate([FromForm] Question question, IFormFile? upload)
        {
            var memberId = "a8e36451-c3fb-44ba-a05e-602ca0760166";

            var result = await _user.CreateQuestionAsync(question, upload, memberId);

            return result ? Ok() : BadRequest();
        }

        //問題清單
        [HttpGet("GetMyQuestions")]
        public async Task<IActionResult> GetMyQuestions()
        {
            //var memberID = _userAccessorService.GetMemberId();
            //if (memberID == null) return Unauthorized();

            var memberID = "a8e36451-c3fb-44ba-a05e-602ca0760166";

            var questions = await _user.GetMemberQuestionsAsync(memberID);

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


    }
}
