using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.Service.Sms;
using TicketSalesSystem.ViewModel.Member;

namespace TicketSalesSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "MemberScheme")]
    public class UserMembersController : Controller
    {
        private readonly TicketsContext _context;
        private readonly ISmsService _smsService;
        private readonly IUserAccessorService _userAccessorService;

        public UserMembersController(TicketsContext context, ISmsService smsService, IUserAccessorService userAccessor)
        {
            _context = context;
            _smsService = smsService;
            _userAccessorService = userAccessor;
        }

        public async Task<IActionResult> Index()
        {
            // 1. 取得當前登入玩家的 ID
            string? memberId = _userAccessorService.GetMemberId();
            if (string.IsNullOrEmpty(memberId))
            {
                return RedirectToAction("MemberLogin", "Login");
            }

            // 2. 抓取會員本人物件 (傳給 View 當 Model)
            // 🚩 注意：這解決了之前的 List<Member> 錯誤，只抓「一個」
            var member = await _context.Member
                .FirstOrDefaultAsync(m => m.MemberID == memberId);

            if (member == null) return NotFound();

            // 3. 數據統計區 (用於儀表板方塊)
            ViewBag.OrderCount = await _context.Order.CountAsync(o => o.MemberID == memberId);
            ViewBag.QuestionCount = await _context.Question.CountAsync(q => q.MemberID == memberId);
            ViewBag.MemberName = member.Name;

            // 4. 抓取最新日誌時間點
            
            //總提問數 (All Quests)
            ViewBag.QuestionTotal = await _context.Question
                .CountAsync(q => q.MemberID == memberId);

            //今日新增提問 (Today's Quests)
            var today = DateTime.Today;
            ViewBag.QuestionToday = await _context.Question
                .CountAsync(q => q.MemberID == memberId && q.CreatedTime >= today);

            //最新提問時間
            var latestQuestion = await _context.Question
                .Where(q => q.MemberID == memberId)
                .OrderByDescending(q => q.CreatedTime)
                .Select(q => q.CreatedTime)
                .FirstOrDefaultAsync();

           
            var OrderSum = await _context.Order
                .Where(q => q.MemberID == memberId)
                .CountAsync();

            //總訂單數 (All Order)
            ViewBag.OrderTotal = await _context.Order
                .CountAsync(q => q.MemberID == memberId);

            //今日新增訂單 (Today's Order)            
            ViewBag.QuestionToday = await _context.Order
                .CountAsync(q => q.MemberID == memberId && q.OrderCreatedTime >= today);
            
            //最新訂單時間
            var latestOrder = await _context.Order
                .Where(o => o.MemberID == memberId)
                .OrderByDescending(o => o.OrderCreatedTime)
                .Select(o => o.OrderCreatedTime)
                .FirstOrDefaultAsync();

            // 5. 格式化時間並存入 ViewBag
            ViewBag.LatestQuestionTime = latestQuestion == default ? "---" : latestOrder.ToString("yyyy/MM/dd");
            ViewBag.LatestOrderTime = latestOrder == default ? "---" : latestOrder.ToString("yyyy/MM/dd");

            // 6. 處理登入時間 (從 Session 抓取真正的登入時間)
            ViewBag.LoginTime = HttpContext.Session.GetString("LoginTime")
                                ?? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            // 🚀 回傳 Model
            return View(member);
        }


        // GET: Members/Details/5
        public async Task<IActionResult> UserDetails(string id)
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

        private void PopulateDropdownLists(string? selectedStatus = null)
        {

            ViewData["AccountStatusID"] = new SelectList(_context.AccountStatus.ToList(), "AccountStatusID", "FullDisplayName", selectedStatus);

        }

    }
}
