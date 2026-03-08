using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.User;

namespace TicketSalesSystem.ViewComponents
{
    public class VCLatestNews : ViewComponent
    {
        private readonly TicketsContext _context;
        private readonly IUser _userService;
        public VCLatestNews(TicketsContext context, IUser userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var latestNews =await _userService.GetLatestFiveNoticesAsync();
            return View(latestNews);

        }
    }
}
