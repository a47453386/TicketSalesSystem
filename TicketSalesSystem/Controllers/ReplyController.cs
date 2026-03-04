using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.IUserAccessor;

namespace TicketSalesSystem.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(AuthenticationSchemes = "EmployeeScheme", Roles = "S,A,C")]
    public class ReplyController : Controller
    {
        private readonly TicketsContext _context;
        private readonly IUserAccessorService _userAccessorService;

        public ReplyController(TicketsContext context, IUserAccessorService userAccessorService)
        {
            _context = context;
            _userAccessorService = userAccessorService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string questionId, string replyDescription, string replyStatusId)
        {


            // 1. 內容驗證
            if (string.IsNullOrWhiteSpace(replyDescription))
            {
                TempData["ErrorMessage"] = "回覆內容不能為空！";
                // 確保這裡的參數名稱與路由匹配
                return RedirectToAction("Details", "Questions", new { id = questionId });
            }

            // 2. 檢查問題是否存在
            var question = await _context.Question.FindAsync(questionId);
            if (question == null)
            {
                TempData["ErrorMessage"] = "問題不存在！";
                return RedirectToAction("Index", "Questions");
            }

            var employeeId = _userAccessorService.GetEmployeeId();

            // 3. 建立回覆資料
            var reply = new Reply
            {
                ReplyID = Guid.NewGuid().ToString(),
                QuestionID = questionId,
                ReplyDescription = replyDescription,
                EmployeeID = employeeId,
                CreatedTime = DateTime.Now,
                ReplyStatusID = replyStatusId,
                Note = ""
            };

            try
            {
                _context.Add(reply);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "回覆成功！";
            }
            catch (Exception ex)
            {
                
                TempData["ErrorMessage"] = "發生錯誤：" + ex.Message;
            }

            
            return RedirectToAction("Details", "Questions", new { id = questionId });
        }
    }
}
