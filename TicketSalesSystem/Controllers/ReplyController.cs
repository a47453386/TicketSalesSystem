using Microsoft.AspNetCore.Mvc;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.Controllers
{
    public class ReplyController : Controller
    {
        private readonly TicketsContext _context;

        public ReplyController(TicketsContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendReply(string questionId, string replyDescription)
        {
            if (string.IsNullOrEmpty(replyDescription)) return BadRequest("回覆內容不可為空");

            // 1. 建立回覆資料
            var newReply = new Reply
            {
                ReplyID = Guid.NewGuid().ToString(),
                QuestionID = questionId,
                ReplyDescription = replyDescription,
                EmployeeID = "A23025", // 應抓取登入員工
                CreatedTime = DateTime.Now,
                ReplyStatusID = "2", // 🚩 假設 "2" 代表「已回覆」
                Note = ""
            };

            try
            {
                _context.Add(newReply);

                // 💡 可以在這裡同時更新 Question 的狀態（如果 Question 表有狀態欄位的話）
                // 或者透過 ReplyStatusID 關聯來判斷

                await _context.SaveChangesAsync();

                // 回覆完後回到該問題的詳細頁面
                return RedirectToAction("Details", "Questions", new { id = questionId });
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
    }
}
