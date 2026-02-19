using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TicketsContext _context;

        public HomeController(ILogger<HomeController> logger, TicketsContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var programmes = _context.Programme
                .Where(p => p.ProgrammeStatusID == "O") // 只顯示開賣中的節目
                .OrderBy(p => p.CreatedTime)
                .ToList(); // 確保順序一致
                
            return View(programmes);
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
