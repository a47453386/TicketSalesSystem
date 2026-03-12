using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TicketSalesSystem.DTOs.Login;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.Service.User;
using TicketSalesSystem.ViewModel.Member;
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
        private readonly IUserAccessorService _userAccessorService;

        public HomeApiController(IUser user, TicketsContext context, IUserAccessorService userAccessor)
        {
            _user = user;
            _context = context;
            _userAccessorService = userAccessor;
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
            var result = faqs.Select(f => new
            {
                faqTitle = f.FAQTitle,
                faqDescription = f.FAQDescription,
                faqTypeName = f.FAQType?.FAQTypeName
            }).ToList();
            return Ok(result);
        }





        //我要發問
        [HttpPost("QuestionsCreate")]
        public async Task<IActionResult> QuestionsCreate([FromForm] Question question, IFormFile? upload)
        {
            var memberID = _userAccessorService.GetMemberId();
            if (memberID == null) return Unauthorized();

            var result = await _user.CreateQuestionAsync(question, upload, memberID);

            return result ? Ok() : BadRequest();
        }

        //問題清單



        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            // 1. 基本防呆
            if (request == null || string.IsNullOrEmpty(request.Account))
            {
                return Ok(new LoginResponseDTO { Success = false, Message = "請輸入帳號與密碼" });
            }

            // 2. 尋找登入資訊
            var loginInfo = await _context.MemberLogin
                .Include(m => m.Member)
                .FirstOrDefaultAsync(m => m.Account == request.Account);

            // 3. 檢查帳號是否存在
            if (loginInfo == null)
            {
                return Ok(new LoginResponseDTO { Success = false, Message = "帳號或密碼錯誤" });
            }

            // 4. 驗證密碼
            var hasher = new PasswordHasher<string>();
            var verifyResult = hasher.VerifyHashedPassword(loginInfo.Account, loginInfo.Password, request.Password);

            if (verifyResult != PasswordVerificationResult.Success)
            {
                return Ok(new LoginResponseDTO { Success = false, Message = "帳號或密碼錯誤" });
            }

            // 5. 檢查帳號狀態 (A 為正常)
            if (loginInfo.Member.AccountStatusID != "A")
            {
                return Ok(new LoginResponseDTO { Success = false, Message = "帳號異常或停權，請聯繫客服。" });
            }

            try
            {
                // 🚩 6. 重要：核發身分證 (Cookie)
                // 這步沒做，App 就算記住 ID，呼叫其他 API 也會被擋掉
                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, loginInfo.Member.Name ?? loginInfo.Account),
                        new Claim(ClaimTypes.NameIdentifier, loginInfo.MemberID), // 存入 ID
                        new Claim(ClaimTypes.Role, "Member")
                    };

                var claimsIdentity = new ClaimsIdentity(claims, "MemberScheme");
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) // App 建議給長一點的時間
                };

                // 執行登入
                await HttpContext.SignInAsync("MemberScheme", new ClaimsPrincipal(claimsIdentity), authProperties);

                // 7. 更新最後登入時間
                loginInfo.Member.LastLoginTime = DateTime.Now;
                await _context.SaveChangesAsync();

                // 8. 回傳結果 (包含 ID 讓 App 儲存)
                return Ok(new LoginResponseDTO
                {
                    Success = true,
                    Message = "登入成功",
                    MemberID = loginInfo.MemberID,
                    Name = loginInfo.Member.Name
                });
            }
            catch (Exception ex)
            {
                return Ok(new LoginResponseDTO { Success = false, Message = "伺服器內部錯誤" });
            }
        }


        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] VMMemberCreate vm)
        {
            // 🚩 1. 如果你希望手動處理驗證，請確保這段邏輯能執行
            if (!ModelState.IsValid)
            {
                // 抓出所有錯誤，並組成一個清楚的字串
                var errorMessages = ModelState.Where(ms => ms.Value.Errors.Count > 0)
                    .Select(ms => new {
                        Field = ms.Key, // 哪一個欄位出錯
                        Errors = string.Join(", ", ms.Value.Errors.Select(e => e.ErrorMessage)) // 錯誤原因
                    }).ToList();

                // 打印到偵錯視窗 (Visual Studio 輸出視窗看得到)
                foreach (var err in errorMessages)
                {
                    System.Diagnostics.Debug.WriteLine($"欄位驗證失敗: {err.Field} -> {err.Errors}");
                }

                return Ok(new
                {
                    Success = false,
                    // 回傳第一個最主要的錯誤訊息給 App
                    Message = errorMessages.FirstOrDefault()?.Errors ?? "輸入資料格式有誤",
                    ErrorField = errorMessages.FirstOrDefault()?.Field ?? "General",
                    Details = errorMessages // 包含所有錯誤細節，方便除錯
                });
            }

            // 🚩 2. 呼叫 Service
            var result = await _user.CreateMemberAsync(vm);

            return Ok(new
            {
                Success = result.Success,
                Message = result.Message,
                ErrorField = result.ErrorField,
                MemberID = result.MemberID
            });
        }



    }
}
