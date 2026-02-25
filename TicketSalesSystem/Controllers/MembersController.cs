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

        public MembersController(TicketsContext context)
        {
            _context = context;           
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
