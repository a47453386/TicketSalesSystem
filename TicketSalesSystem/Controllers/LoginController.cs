using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel;

namespace TicketSalesSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly TicketsContext _context;
        public LoginController(TicketsContext context)
        {
            _context = context;
        }

        // 🚩 GET: 登入頁面 / 彈窗內容
        [HttpGet]
        public IActionResult MemberLogin(string? returnUrl)
        {
            // 1. 如果使用者已經登入，直接踢回首頁或 ReturnUrl
            if (User.Identity.IsAuthenticated)
            {
                return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
            }

            // 2. 準備資料給 View
            ViewBag.ReturnUrl = returnUrl;
            var vm = new VMLogin(); // 🚩 務必傳入空物件，避免 PartialView 報 Null 錯誤

            // 3. 判斷是否為 AJAX 請求 (彈窗使用)
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // 指定完整路徑確保找得到檔案
                return PartialView("~/Views/Shared/_LoginPartial.cshtml", vm);
            }

            return View(vm);
        }

        // 🚩 POST: 處理登入驗證
        [HttpPost]
        [ValidateAntiForgeryToken] // 安全性檢查
        public async Task<IActionResult> MemberLogin(VMLogin vm, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            // 1. 基本欄位驗證 (如：必填)
            if (!ModelState.IsValid)
            {
                return isAjax ? PartialView("_LoginPartial", vm) : View(vm);
            }

            // 2. 比對資料庫 (帳號與密碼)
            var user = await _context.MemberLogin
                .FirstOrDefaultAsync(m => m.Account == vm.Account && m.Password == vm.Password);

            if (user != null)
            {
                // 3. 準備身分證 (Claims)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Account),
                    new Claim(ClaimTypes.NameIdentifier, user.MemberID),
                    new Claim(ClaimTypes.Role, "User") // 標記為一般會員
                };

                var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");

                // 4. 執行登入 (核發 Cookie)
                await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
                {
                    IsPersistent = true, // 瀏覽器關閉後是否記住我
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1) // 一天內免登入
                });

                // 5. 處理回傳結果
                if (isAjax)
                {
                    return Json(new { success = true, redirectUrl = returnUrl ?? Url.Action("Index", "Home") });
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            // 6. 登入失敗：加入錯誤訊息
            ModelState.AddModelError(string.Empty, "帳號或密碼錯誤，請重新輸入。");

            return isAjax ? PartialView("_LoginPartial", vm) : View(vm);
        }

        // 🚩 GET: 登出
        public async Task<IActionResult> MemberLogout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }
    }
}
