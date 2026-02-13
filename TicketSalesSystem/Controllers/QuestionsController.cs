using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly TicketsContext _context;

        public QuestionsController(TicketsContext context)
        {
            _context = context;
        }

        // A. 我的提問紀錄 (消費者只能看自己的)
        public async Task<IActionResult> MyList()
        {
            // 🚩 這裡暫時寫死，之後要從 User.Identity 或 Session 抓 MemberID
            string currentMemberId = "004bc90a-26fb-48e9-a762-653a232d86e2";

            var myQuestions = await _context.Question
                .Include(q => q.QuestionType)
                .Include(q => q.Reply) // 載入回覆紀錄
                .Where(q => q.MemberID == currentMemberId)
                .OrderByDescending(q => q.CreatedTime)
                .ToListAsync();

            return View(myQuestions);
        }

        public async Task<IActionResult> Create()
        {

            return View();
        }

        // B. 提交新問題 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question question)
        {
            // 🚩 後端補足必要資訊
            question.QuestionID = Guid.NewGuid().ToString();
            question.CreatedTime = DateTime.Now;
            question.MemberID = "004bc90a-26fb-48e9-a762-653a232d86e21"; // 應抓取登入者

            // 移除不需要前端輸入的驗證
            ModelState.Remove("QuestionID");
            ModelState.Remove("MemberID");
            ModelState.Remove("CreatedTime");

            if (ModelState.IsValid)
            {
                _context.Add(question);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyList));
            }
            return View(question);
        }

        // GET: Questions/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var question = await _context.Question
                .Include(q => q.QuestionType)
                .Include(q => q.Member)
                .Include(q => q.Reply.OrderBy(r => r.CreatedTime)) // 按時間順序排列回覆
                    .ThenInclude(r => r.Employee) // 載入是哪位員工回覆的
                .Include(q => q.Reply)
                    .ThenInclude(r => r.ReplyStatus) // 載入回覆狀態名稱
                .FirstOrDefaultAsync(m => m.QuestionID == id);

            if (question == null) return NotFound();

            // 安全檢查：確保消費者只能看自己的問題
            // string currentMemberId = "MEMBER_001"; 
            // if (question.MemberID != currentMemberId) return Forbid();

            return View(question);
        }

    }
}
