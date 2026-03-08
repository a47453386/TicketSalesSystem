using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.ID;
using TicketSalesSystem.Service.User;

namespace TicketSalesSystem.Controllers
{
   
    public class UserFAQsController : Controller
    {
        private readonly TicketsContext _context;
        private readonly IUser _userService;

        public UserFAQsController(TicketsContext context, IUser userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<IActionResult> UserIndex()
        {
            var faqs = await _userService.GetFAQsAsync();
            return View(faqs);
        }


    }
}
