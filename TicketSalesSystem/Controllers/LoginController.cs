using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Claims;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.ViewModel;

namespace TicketSalesSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly TicketsContext _context;
        private readonly IUserAccessorService _userAccessorService;
        public LoginController(TicketsContext context, IUserAccessorService userAccessorService)
        {
            _context = context;
            _userAccessorService = userAccessorService;
        }

        // 🚩 GET: 登入頁面 / 彈窗內容
        [HttpGet]
        public IActionResult MemberLogin(string? returnUrl)
        {
            // 1. 如果使用者已經登入，直接踢回首頁或 ReturnUrl
            if (_userAccessorService.IsAuthenticated())
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MemberLogin(VMLogin vm, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (!ModelState.IsValid)
            {
                return isAjax ? PartialView("_LoginPartial", vm) : View(vm);
            }

            // 🚩 修改 1：只用「帳號」去找人。
            // 因為資料庫裡的密碼是加密的，直接比對 vm.Password 會找不到。
            var user = await _context.MemberLogin
                .FirstOrDefaultAsync(m => m.Account == vm.Account);

            // 🚩 修改 2：找到人後，才開始比對密碼
            if (user != null)
            {
                var hasher = new PasswordHasher<string>();

                // 驗證輸入的密碼 (vm.Password) 與資料庫存的加密密碼 (user.Password)
                var result = hasher.VerifyHashedPassword(vm.Account, user.Password, vm.Password);

                // 🚩 修改 3：只有驗證成功 (Success) 才執行登入邏輯
                if (result == PasswordVerificationResult.Success)
                {
                    // 3. 準備身分證 (Claims)
                    var claims = new List<Claim>
                     {
                         new Claim(ClaimTypes.Name, user.Account),
                         new Claim(ClaimTypes.NameIdentifier, user.MemberID),
                         new Claim(ClaimTypes.Role, "User")
                     };

                    var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");

                    // 4. 執行登入 (核發 Cookie)
                    await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
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
            }

            // 🚩 修改 4：不管是「找不到人」還是「密碼錯」，統一口徑 (安全性考量)
            ModelState.AddModelError(string.Empty, "帳號或密碼錯誤，請重新輸入。");
            return isAjax ? PartialView("_LoginPartial", vm) : View(vm);
        }



        // 🚩 GET: 員工登入頁面
        [HttpGet]
        public async Task<IActionResult> EmployeeLogin(string? returnUrl) // 1. 補上 async Task
        {
            // 使用你寫好的 Service 檢查狀態
            if (_userAccessorService.IsAuthenticated())
            {
                // 2. 直接拿 Role，不需要手動去 Claims 抓
                var role = _userAccessorService.GetUserRole();
                var validRoles = new[] { "S", "A", "B", "C","F" };

                if (role != null && validRoles.Contains(role))
                {
                    // 3. 🚩 這裡可以選擇：要「相信 Cookie」直接跳轉，還是「再次查 DB」
                    // 既然你之前想去資料庫找，我們就保留這個嚴謹的檢查
                    var empId = _userAccessorService.GetEmployeeId();
                    var dbRole = await _context.Employee
                        .Where(e => e.EmployeeID == empId)
                        .Select(e => e.RoleID)
                        .FirstOrDefaultAsync();

                    if (dbRole != null && validRoles.Contains(dbRole))
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new VMLogin());
        }

        // 🚩 POST: 員工登入驗證
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeLogin(VMLogin vm, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid) return View(vm);

            // 🚩 修改 1：查【員工登入】表，Include 員工主表拿 RoleID 與 Name
            var emp = await _context.EmployeeLogin
                .Include(e => e.Employee)
                .FirstOrDefaultAsync(e => e.Account == vm.Account);

            if (emp != null)
            {
                var hasher = new PasswordHasher<string>();
                var result = hasher.VerifyHashedPassword(vm.Account, emp.Password, vm.Password);

                if (result == PasswordVerificationResult.Success)
                {
                    // 🚩 修改 2：準備「員工身分證」
                    // 注意：這裡使用 emp.Employee.Name，這樣導覽列才會顯示「陳信宏」而不是「admin001」
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, emp.Employee.Name ?? emp.Account),
                        new Claim(ClaimTypes.NameIdentifier, emp.EmployeeID),
                        new Claim(ClaimTypes.Role, emp.Employee.RoleID) // S, A, B, C
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");

                    // 🚩 修改 3：核發 Cookie
                    await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                    });

                    // 🚩 修改 4：跳轉邏輯
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Dashboard", "Admin");
                }
            }

            // 統一錯誤訊息
            ModelState.AddModelError(string.Empty, "員工帳號或密碼錯誤。");
            return View(vm);
        }







        // 🚩 GET: 登出
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");

            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}
