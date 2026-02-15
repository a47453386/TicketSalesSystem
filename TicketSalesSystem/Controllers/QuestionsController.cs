using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.Images;

namespace TicketSalesSystem.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly TicketsContext _context;
        private readonly IFileService _fileService;

        public QuestionsController(TicketsContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            // 🚩 必須 Include(q => q.Reply)，畫面的 Count 判斷才會有值
            var questions = await _context.Question
                .Include(q => q.Member)
                .Include(q => q.QuestionType)
                .Include(q => q.Reply)
                .OrderByDescending(q => q.CreatedTime)
                .ToListAsync();

            return View(questions);
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
            PopulateDropdownLists();
            return View();
        }

        // B. 提交新問題 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question question,IFormFile? upload)
        {
            // 🚩 後端補足必要資訊
            question.QuestionID = Guid.NewGuid().ToString();
            question.CreatedTime = DateTime.Now;
            question.MemberID = "004bc90a-26fb-48e9-a762-653a232d86e2"; // 應抓取登入者


            // 如果沒有上傳檔案，確保 UploadFile 欄位為 null
            if (upload !=null && upload.Length != 0)
            {
                string dbPath = await _fileService.SaveFileAsync(upload, "Questions");
                question.UploadFile = dbPath;
            }

            

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


            PopulateDropdownLists(question);
            
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

            ViewData["ReplyStatusID"] = new SelectList(_context.ReplyStatus.ToList(), "ReplyStatusID", "ReplyStatusName");
            return View(question);
        }

        private void PopulateDropdownLists(Question q=null )
        {
           ViewData["QuestionTypeID"] = new SelectList(_context.QuestionType.ToList(), "QuestionTypeID", "QuestionTypeName",q?.QuestionTypeID );
        }




        
    }
}
