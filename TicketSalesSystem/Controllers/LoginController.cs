using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Claims;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.Service.User;
using TicketSalesSystem.ViewModel;
using TicketSalesSystem.ViewModel.Login;

namespace TicketSalesSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly TicketsContext _context;
        private readonly IUserAccessorService _userAccessorService;
        private readonly IDataProtector _dataProtector;
        private readonly PasswordHasher<MemberLogin> _passwordHasher;

        public LoginController(TicketsContext context, IUserAccessorService userAccessorService, IDataProtectionProvider dataProtector, PasswordHasher<MemberLogin> passwordHasher)
        {
            _context = context;
            _userAccessorService = userAccessorService;
            _dataProtector = dataProtector.CreateProtector("PasswordResetPurpose"); ;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult MemberLogin(string? returnUrl)
        {
            //檢查是否已有會員證 (這段寫得很好，保留)
            if (_userAccessorService.IsMember())
            {
                return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
            }

            ViewBag.ReturnUrl = returnUrl;
            var vm = new VMLogin();

            //檢查是否為 AJAX 請求 (彈窗呼叫)
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // 回傳 PartialView，這會徹底無視 _ViewStart 和所有 Layout
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

            //用「帳號」去找人。
            // 因為資料庫裡的密碼是加密的，直接比對 vm.Password 會找不到。
            var user = await _context.MemberLogin
                .Include(m => m.Member)
                .FirstOrDefaultAsync(m => m.Account == vm.Account);

            // 找到人後，才開始比對密碼
            if (user != null)
            {
                var hasher = new PasswordHasher<string>();

                // 驗證輸入的密碼 (vm.Password) 與資料庫存的加密密碼 (user.Password)
                var result = hasher.VerifyHashedPassword(vm.Account, user.Password, vm.Password);

                // 驗證成功 (Success) 才執行登入邏輯
                if (result == PasswordVerificationResult.Success)
                {
                    // 檢查帳號狀態 (停權、異常等)
                    if (user.Member.AccountStatusID != "A")
                    {
                        ModelState.AddModelError(string.Empty, "您的帳號目前處於停權或異常狀態，請聯繫客服人員。");
                        return isAjax ? PartialView("_LoginPartial", vm) : View(vm);
                    }
                    //更新最後登入時間
                    user.Member.LastLoginTime = DateTime.Now;

                    //存回資料庫 (非同步)
                    await _context.SaveChangesAsync();

                    //準備身分證 (Claims)
                    var claims = new List<Claim>
                     {
                         new Claim(ClaimTypes.Name, user.Member.Name?? user.Account),// 這裡放姓名
                         new Claim(ClaimTypes.NameIdentifier, user.MemberID),// 這裡放 ID
                         new Claim(ClaimTypes.Role, "Member")//會員 (Member)
                     };

                    var claimsIdentity = new ClaimsIdentity(claims, "MemberScheme");

                    //執行登入 (核發 Cookie)
                    await HttpContext.SignInAsync("MemberScheme", new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5),
                        AllowRefresh = true
                    });

                    //處理回傳結果
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

            // 不管是「找不到人」還是「密碼錯」，統一口徑 (安全性考量)
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
                    // 檢查帳號狀態 (停權、異常等)
                    if (emp.Employee.AccountStatusID != "A")
                    {
                        ModelState.AddModelError(string.Empty, "此員工帳號已被停用，請洽詢系統管理員。");
                        return RedirectToAction("EmployeeLogin", "Login");
                    }

                    // 🚩 1. 更新最後登入時間
                    emp.Employee.LastLoginTime = DateTime.Now;

                    // 🚩 2. 存回資料庫 (非同步)
                    await _context.SaveChangesAsync();

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
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5),
                        AllowRefresh = true
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




        //會員登出
        public async Task<IActionResult> MemberLogout()
        {
            await HttpContext.SignOutAsync("MemberScheme");
            // 不需要清除整個 Session，因為員工可能還在線
            // HttpContext.Session.Remove("SomeMemberData"); 
            return RedirectToAction("Index", "Home");
        }

        //員工登出
        public async Task<IActionResult> EmployeeLogout()
        {
            await HttpContext.SignOutAsync("EmployeeScheme");
            return RedirectToAction("Dashboard", "Admin");
        }

        //會員忘記密碼(發送重設連結)
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var member = await _context.MemberLogin
                .Include(m=>m.Member)
                .FirstOrDefaultAsync(m => m.Member.Email == email);

            if (member == null)
            {
                 ModelState.AddModelError("email", "無此信箱，請確認信箱，並重新填寫!!!!");
                return View();
            }
            // 1. 採集指紋 (取現有雜湊密碼的最後 8 碼作為 Security Stamp)
            string stamp = member.Password.Substring(member.Password.Length - 8);

            // 2. 封裝封包：ID | 過期時間(Ticks) | 指紋
            string payload = $"{member.MemberID}|{DateTime.UtcNow.AddMinutes(30).Ticks}|{stamp}";

            // 3. 加密成 Token
            string token = _dataProtector.Protect(payload);

            // 4. 產生修復連結 (導向 ResetPassword)
            string resetLink = Url.Action("ResetPassword", "Login", new { t = token }, Request.Scheme);

            // TODO: 串接 MailService 發信
            // _mailService.SendEmail(member.Email, "【系統】帳號存取權修復指令", $"連結：{resetLink}");

            // 暫時在 Console 打印，方便你測試
            Console.WriteLine($"[DEBUG] Reset Link: {resetLink}");
            ViewBag.IsSuccess = true;
            ViewBag.Message = "已發送連結，請確認信箱。";
            return View();
        }

        //重設密碼
        [HttpGet]
        public IActionResult ResetPassword(string t)
        {
            if (string.IsNullOrEmpty(t)) return RedirectToAction("MemberLogin");

            // 將 Token 填入 ViewModel 傳給前端隱藏欄位
            var vm = new VMResetPassword { Token = t };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(VMResetPassword vm)
        {
            if (!ModelState.IsValid) return View(vm);

            try
            {
                // 1. 解密憑證
                string decryptedData = _dataProtector.Unprotect(vm.Token);
                var parts = decryptedData.Split('|');

                string memberId = parts[0];
                long expireTicks = long.Parse(parts[1]);
                string oldStamp = parts[2];

                // 2. 效期檢查
                if (DateTime.UtcNow.Ticks > expireTicks)
                {
                    ModelState.AddModelError("", "修復憑證已失效，請重新申請。");
                    return View(vm);
                }

                // 3. 指紋核對 (確保密碼未曾被改過)
                var member = await _context.MemberLogin
                    .Include(m=>m.Member)
                    .FirstOrDefaultAsync(m => m.Member.MemberID == memberId);

                if (member == null) return NotFound();

                string currentStamp = member.Password.Substring(member.Password.Length - 8);
                if (currentStamp != oldStamp)
                {
                    ModelState.AddModelError("", "此連結已過期或已使用過 (Security Stamp Mismatch)。");
                    return View(vm);
                }

                // 4. 通過驗證：執行密碼覆蓋
                member.Password = _passwordHasher.HashPassword(member, vm.NewPassword);

                await _context.SaveChangesAsync();

                return RedirectToAction("MemberLogin", new { msg = "SUCCESS: 通行碼已更新！" });
            }
            catch
            {
                // 加密格式不對或被 竄改會進這裡
                ModelState.AddModelError("", "無效的修復協定。");
                return View(vm);
            }
        }


    }
}
