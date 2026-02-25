using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.Images;
using TicketSalesSystem.Service.IUserAccessor;

namespace TicketSalesSystem.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly TicketsContext _context;
        private readonly IFileService _fileService;
        private readonly IUserAccessorService _userAccessorService;

        public QuestionsController(TicketsContext context, IFileService fileService,IUserAccessorService userAccessor)
        {
            _context = context;
            _fileService = fileService;
            _userAccessorService = userAccessor;
        }

        [Authorize(AuthenticationSchemes = "EmployeeScheme", Roles = "S,A,C")]
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




       

       
        [Authorize(AuthenticationSchemes = "EmployeeScheme", Roles = "S,A,C")]
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

            
            ViewData["ReplyStatusID"] = new SelectList(_context.ReplyStatus.ToList(), "ReplyStatusID", "ReplyStatusName");
            return View(question);
        }

        private void PopulateDropdownLists(Question q=null )
        {
           ViewData["QuestionTypeID"] = new SelectList(_context.QuestionType.ToList(), "QuestionTypeID", "QuestionTypeName",q?.QuestionTypeID );
        }




        
    }
}
