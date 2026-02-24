using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
 using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.Sms;
using TicketSalesSystem.ViewModel.Member;

namespace TicketSalesSystem.Controllers
{
    public class MembersController : Controller
    {
        private readonly TicketsContext _context;
        private readonly ISmsService _smsService;

        public MembersController(TicketsContext context, ISmsService smsService)
        {
            _context = context;
            _smsService = smsService;
        }

        // GET: Members
        public async Task<IActionResult> Index(string searchString)
        {
            var members = _context.Member
                .Include(m => m.AccountStatus)
                .OrderByDescending(m => m.CreatedDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                members = members.Where(s => s.Name.Contains(searchString)
                                             || s.MemberID.Contains(searchString)
                                             || s.Tel.Contains(searchString) // 🚩 增加手機搜尋
                                             || (s.Email != null && s.Email.Contains(searchString)));
            }

            var result = await members.OrderByDescending(m => m.CreatedDate).ToListAsync();
            ViewData["CurrentFilter"] = searchString;

            return View(result);
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member
                .Include(m => m.AccountStatus)
                .FirstOrDefaultAsync(m => m.MemberID == id);
            if (member == null)
            {
                return NotFound();
            }

            var memberLogin = await _context.MemberLogin.FindAsync(id);

            ViewData["Account"] = memberLogin?.Account ?? "無帳號資訊";

            return View(member);
        }



        [Authorize(AuthenticationSchemes = "MemberScheme")]
        // GET: Members/Create
        public IActionResult Create()
        {
            PopulateDropdownLists();
            return View();
        }

        // POST: Members/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VMMemberCreate vm)
        {
            //檢查手機號碼是否已存在
            bool isTelExist = await _context.Member.AnyAsync(m => m.Tel == vm.Tel);

            if (isTelExist)
            {
                // 針對 Tel 欄位加入錯誤訊息
                ModelState.AddModelError("Tel", "此手機號碼已被使用，請更換號碼或嘗試找回帳號。");
            }

            if (!ModelState.IsValid)
            {
                PopulateDropdownLists(vm.AccountStatusID);
                return View(vm);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    string newMemberID = Guid.NewGuid().ToString();

                    var member = new Member
                    {
                        MemberID = newMemberID,
                        Name = vm.Name,
                        Address = vm.Address,
                        Birthday = vm.Birthday,
                        Tel = vm.Tel,
                        Gender = vm.Gender,
                        NationalID = vm.NationalID,
                        Email = vm.Email,
                        CreatedDate = DateTime.Now,
                        LastLoginTime = null,
                        IsPhoneVerified = false,
                        AccountStatusID = "A"
                    };
                    _context.Member.Add(member);

                    var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<string>();
                    var memberLogin = new MemberLogin
                    {
                        MemberID = newMemberID,
                        Account = vm.Account,
                        Password = hasher.HashPassword(vm.Account, vm.Password)
                    };

                    _context.MemberLogin.Add(memberLogin);


                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Index));


                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "新增會員失敗，錯誤原因：" + ex.Message);
                }


            }

            PopulateDropdownLists(vm.AccountStatusID);
            return View(vm);

        }

        //新增：發送 SMS 驗證碼
        [HttpPost]
        public async Task<IActionResult> SendSmsCode(string id)
        {
            var member = await _context.Member.FindAsync(id);
            if (member == null) return Json(new { success = false, message = "找不到會員資料" });

            // 生成 6 位數驗證碼
            string code = new Random().Next(100000, 999999).ToString();

            // 🚩 存入 Session (Key 包含 MemberID 確保唯一)
            HttpContext.Session.SetString($"SMS_CODE_{id}", code);
            // 紀錄發送時間，可用於判斷 5 分鐘內有效
            HttpContext.Session.SetString($"SMS_TIME_{id}", DateTime.Now.ToString());

            // 呼叫你的 MockSmsService (這會印在 Debug 視窗)
            await _smsService.SendVerificationCodeAsync(member.Tel, code);

            return Json(new { success = true, message = "驗證碼已發送至您的手機" });
        }

        //核對驗證碼
        [HttpPost]
        public async Task<IActionResult> SubmitVerifyCode(string id, string inputCode)
        {
            string? savedCode = HttpContext.Session.GetString($"SMS_CODE_{id}");

            if (string.IsNullOrEmpty(savedCode))
                return Json(new { success = false, message = "驗證碼已過期，請重新取得" });

            if (inputCode == savedCode)
            {
                // 🚩 驗證成功：更新資料庫
                var member = await _context.Member.FindAsync(id);
                if (member != null)
                {
                    member.IsPhoneVerified = true;
                    await _context.SaveChangesAsync();

                    // 清除 Session
                    HttpContext.Session.Remove($"SMS_CODE_{id}");
                    return Json(new { success = true, message = "手機認證成功！" });
                }
            }

            return Json(new { success = false, message = "驗證碼錯誤，請重新輸入" });
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> UserEdit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            var vm = new VMMemberUserEdit
            {
                MemberID = member.MemberID,
                Name = member.Name,
                Address = member.Address,
                Birthday = member.Birthday,
                Tel = member.Tel,
                Gender = member.Gender,
                NationalID = member.NationalID,
                Email = member.Email,
                Account = (await _context.MemberLogin.FindAsync(id))?.Account,
                Password = "" // 密碼不應該被預填充，保持空白
            };

            return View(vm);
        }

        // POST: Members/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserEdit(string id, VMMemberUserEdit vm)
        {
            if (id != vm.MemberID) return NotFound();

            //檢查手機號碼是否已存在
            bool isTelExist = await _context.Member.AnyAsync(m => m.Tel == vm.Tel);

            if (isTelExist)
            {
                // 針對 Tel 欄位加入錯誤訊息
                ModelState.AddModelError("Tel", "此手機號碼已被使用，請更換號碼或嘗試找回帳號。");
            }

            ModelState.Remove("NationalID");
            ModelState.Remove("Account");
            ModelState.Remove("Password");

            if (!ModelState.IsValid) return View(vm);
          

            var member = await _context.Member.FindAsync(vm.MemberID);
            if (member == null) return NotFound();
          

            try
            {
                //處理手機號碼變更邏輯 (如果手機換了，建議重設驗證狀態)
                if (member.Tel != vm.Tel)
                {
                    member.IsPhoneVerified = false;
                }

                member.Address = vm.Address;
                member.Tel = vm.Tel;
                member.Email = vm.Email;

                await _context.SaveChangesAsync();
                TempData["Success"] = "個人資料已更新";
                return RedirectToAction("UserEdit", new { id = vm.MemberID });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "會員資料更新失敗，錯誤原因：" + ex.Message);
            }

            return View(vm);
        }




        public async Task<IActionResult> AdminEdit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
             
            var memberLogin = await _context.MemberLogin.FindAsync(id);
            if (memberLogin == null) return NotFound();

            var vm = new VMMemberAdminEdit
            {
                MemberID = member.MemberID,
                Name = member.Name,
                Account = memberLogin?.Account,
                Address = member.Address,
                Birthday = member.Birthday,
                Tel = member.Tel,
                Gender = member.Gender,
                NationalID = member.NationalID,
                Email = member.Email,
                IsPhoneVerified = member.IsPhoneVerified,
                AccountStatusID = member.AccountStatusID
            };

            PopulateDropdownLists(vm.AccountStatusID);
            return View(vm);
        }

        // POST: Members/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminEdit(string id, VMMemberAdminEdit vm)
        {
            if (id != vm.MemberID) return NotFound();

            ModelState.Remove("Account");
            ModelState.Remove("Password");
            ModelState.Remove("Tel");
            ModelState.Remove(" ");
            if (!ModelState.IsValid) return View(vm);


            var member = await _context.Member.FindAsync(vm.MemberID);
            if (member == null) return NotFound();


            try
            {
                member.Name = vm.Name;
                member.Birthday= vm.Birthday;
                member.Gender= vm.Gender;
                member.NationalID= vm.NationalID;
                member.IsPhoneVerified= vm.IsPhoneVerified;
                member.AccountStatusID= vm.AccountStatusID;

                await _context.SaveChangesAsync();
                TempData["Success"] = "會員資料已更新";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "會員資料更新失敗，錯誤原因：" + ex.Message);
            }
            PopulateDropdownLists(vm.AccountStatusID);
            return View(vm);
        }















        
        // POST: Members/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                // 1. 抓出會員資料
                var member = await _context.Member.FindAsync(id);
                if (member == null) return Json(new { success = false, message = "找不到該會員資料。" });

                // 🚩 2. 安全檢查：如果會員已經有訂單 (Order)，通常禁止刪除以維護報表正確性
                bool hasOrders = await _context.Order.AnyAsync(o => o.MemberID == id);
                if (hasOrders)
                {
                    return Json(new { success = false, message = "此會員已有購票紀錄，無法刪除！建議改為停權狀態。" });
                }

                // 3. 處理關聯的登入帳號 (MemberLogin)
                var login = await _context.MemberLogin.FirstOrDefaultAsync(l => l.MemberID == id);
                if (login != null)
                {
                    _context.MemberLogin.Remove(login);
                }

                // 4. 執行會員主表刪除
                _context.Member.Remove(member);

                // 🚩 核心修正：一定要補上 await，確保存檔完成才回傳
                await _context.SaveChangesAsync();

                // 🚩 5. 回傳 JSON 成功訊號 (因為沒圖片，直接回傳即可)
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "刪除失敗，原因：" + ex.Message });
            }
        }



        private void PopulateDropdownLists(string? selectedStatus = null)
        {

            ViewData["AccountStatusID"] = new SelectList(_context.AccountStatus.ToList(), "AccountStatusID", "FullDisplayName", selectedStatus);
            
        }
    }
}
