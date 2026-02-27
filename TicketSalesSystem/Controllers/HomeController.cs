using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "MemberScheme,EmployeeScheme")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TicketsContext _context;

        public HomeController(ILogger<HomeController> logger, TicketsContext context)
        {
            _logger = logger;
            _context = context;
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            var programmes = _context.Programme
                .Where(p => p.ProgrammeStatusID == "O") // 只顯示開賣中的節目
                .OrderBy(p => p.CreatedTime)
                .ToList(); // 確保順序一致
                
            return View(programmes);
        }

        
        [HttpGet]
        public async Task<IActionResult> GetPublicServerStatus()
        {
            // 1. 判斷人流 (過去 5 分鐘內的訂單嘗試次數)
            var fiveMinutesAgo = DateTime.Now.AddMinutes(-5);
            var activeFlow = await _context.Order.CountAsync(o => o.OrderCreatedTime >= fiveMinutesAgo);

            // 2. 定義對外顯示的字串 (不給具體數字，增加神祕感與安全性)
            string loadStatus = activeFlow > 100 ? "HEAVY" : (activeFlow > 30 ? "MODERATE" : "STABLE");

            // 3. 模擬一個伺服器節點名稱 (增加像素風的氛圍)
            string serverNode = "NODE-TW-03";

            return Json(new
            {
                load = loadStatus,
                node = serverNode,
                uptime = "99.98%",
                timestamp = DateTime.Now.ToString("HH:mm:ss")
            });
        }





        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
