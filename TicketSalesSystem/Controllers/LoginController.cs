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

        [HttpGet]
        public IActionResult MemberLogin(string? returnUrl)
        {
            // 🚩 1. 檢查是否已有會員證 (這段寫得很好，保留)
            if (_userAccessorService.IsMember())
            {
                return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
            }

            ViewBag.ReturnUrl = returnUrl;
            var vm = new VMLogin();

            // 🚩 2. 關鍵判斷：檢查是否為 AJAX 請求 (彈窗呼叫)
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // 🚀 回傳 PartialView，這會徹底無視 _ViewStart 和所有 Layout
                return PartialView("_LoginPartial", vm);
            }

            //如果是直接開啟網頁 (例如：localhost/Login/MemberLogin)
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
                .Include(m => m.Member)
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
                         new Claim(ClaimTypes.Name, user.Member.Name?? user.Account),// 這裡放姓名
                         new Claim(ClaimTypes.NameIdentifier, user.MemberID),// 這裡放 ID
                         new Claim(ClaimTypes.Role, "Member")//會員 (Member)
                     };

                    var claimsIdentity = new ClaimsIdentity(claims, "MemberScheme");

                    // 4. 執行登入 (核發 Cookie)
                    await HttpContext.SignInAsync("MemberScheme", new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
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
            // 🚩 修正：明確檢查是否已經有「員工證」
            if (_userAccessorService.IsEmployee())
            {
                return RedirectToAction("Dashboard", "Admin");
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

                    var claimsIdentity = new ClaimsIdentity(claims, "EmployeeScheme");

                    // 🚩 修改 3：核發 Cookie
                    await HttpContext.SignInAsync("EmployeeScheme", new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
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







        // 🚩 會員登出 (GAME OVER)
        public async Task<IActionResult> MemberLogout()
        {
            await HttpContext.SignOutAsync("MemberScheme");
            // 不需要清除整個 Session，因為員工可能還在線
            // HttpContext.Session.Remove("SomeMemberData"); 
            return RedirectToAction("Index", "Home");
        }

        // 🚩 員工登出 (下班)
        public async Task<IActionResult> EmployeeLogout()
        {
            await HttpContext.SignOutAsync("EmployeeScheme");
            return RedirectToAction("Dashboard", "Admin");
        }
    }
}
