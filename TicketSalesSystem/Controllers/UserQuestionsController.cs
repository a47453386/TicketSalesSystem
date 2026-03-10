using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.Images;
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.Service.User;
using TicketSalesSystem.ViewModel.Question;

namespace TicketSalesSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "MemberScheme")]
    public class UserQuestionsController : Controller
    {
        private readonly TicketsContext _context;
        private readonly IFileService _fileService;
        private readonly IUserAccessorService _userAccessorService;
        private readonly IUser _userService;    

        public UserQuestionsController(TicketsContext context,
            IFileService fileService, IUserAccessorService userAccessor, 
            IUser userService)
        {
            _context = context;
            _fileService = fileService;
            _userAccessorService = userAccessor;
            _userService = userService;
        }

        // A. 我的提問紀錄 (消費者只能看自己的)
        public async Task<IActionResult> MyList()
        {
            //// 🚩 這裡暫時寫死，之後要從 User.Identity 或 Session 抓 MemberID
            //string currentMemberId = "004bc90a-26fb-48e9-a762-653a232d86e2";
            var memberID = _userAccessorService.GetMemberId();

            if (memberID == null)
            {
                return RedirectToAction("MemberLogin", "Login");
            }

            var myQuestions = await _userService.GetMemberQuestionsAsync(memberID);

            return View(myQuestions);
        }
        public async Task<IActionResult> Create()
        {
            PopulateDropdownLists();
            return View();
        }

        // B. 提交新問題 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question question, IFormFile? upload)
        {
            // 1. 取得會員 ID (這是 Web 層的權限檢查)
            var memberID = _userAccessorService.GetMemberId();
            if (memberID == null)
            {
                return RedirectToAction("MemberLogin", "Login");
            }

            // 2. 移除不需要前端驗證的欄位 (這些由 Service 補齊)
            ModelState.Remove("QuestionID");
            ModelState.Remove("MemberID");
            ModelState.Remove("CreatedTime");

            // 3. 呼叫抽離出來的 Service
            if (ModelState.IsValid)
            {
                bool isSuccess = await _userService.CreateQuestionAsync(question, upload, memberID);
                if (isSuccess)
                {
                    return RedirectToAction(nameof(MyList));
                }
            }

            // 失敗則重新填充下拉選單並回傳 View
            PopulateDropdownLists(question);
            return View(question);
        }

        // GET: Questions/Details/5
        public async Task<IActionResult> UserDetails(string id)
        {
            var memberID = _userAccessorService.GetMemberId();
            if (memberID == null) return RedirectToAction("MemberLogin", "Login");

            // 1. 從 Service 拿到 DTO (資料本體)
            var dto = await _userService.GetQuestionDetailForUserAsync(id, memberID);
            if (dto == null) return NotFound();

            // 2. 🚩 在這裡將 DTO 轉為 View 想要的 VMQuestionDetail
            var viewModel = new VMQuestionDetail
            {
                OrderID = dto.OrderID,
                QuestionID = dto.QuestionID,
                QuestionTitle = dto.QuestionTitle,
                QuestionDescription = dto.QuestionDescription,
                CreatedTime = dto.CreatedTime ?? DateTime.Now,
                UploadFile = dto.UploadFile,
                QuestionTypeName = dto.QuestionTypeName,
                Reply = dto.Reply.Select(r => new VMReply
                {
                    ReplyStatusID = r.ReplyStatusID,
                    ReplyDescription = r.ReplyDescription,
                    ReplyStatusName = r.ReplyStatusName,
                    EmployeeName = r.EmployeeName,
                    CreatedTime = r.CreatedTime
                }).ToList()
            };

            return View(viewModel);
        }

        private void PopulateDropdownLists(Question q = null)
        {
            ViewData["QuestionTypeID"] = new SelectList(_context.QuestionType.ToList(), "QuestionTypeID", "QuestionTypeName", q?.QuestionTypeID);
        }

    }
}
