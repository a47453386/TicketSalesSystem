using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.Service.Queue;
using TicketSalesSystem.Service.User;
using TicketSalesSystem.ViewModel;
using TicketSalesSystem.ViewModel.Booking;
using TicketSalesSystem.ViewModel.Order;

namespace TicketSalesSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "MemberScheme")]
    public class UserOrdersController : Controller
    {
        private readonly TicketsContext _context;
        private readonly IQueueService _queueService;
        private readonly IUserAccessorService _userAccessorService;
        private readonly IUser _user;
        public UserOrdersController(TicketsContext context, IQueueService queueService
            , IUserAccessorService userAccessorService, IUser user)
        {
            _context = context;
            _queueService = queueService;
            _userAccessorService = userAccessorService;
            _user = user;
        }

        // 前台會員專區的訂單列表
        public async Task<IActionResult> UserIndex()
        {
            var memberID = _userAccessorService.GetMemberId();

            if (memberID == null)
            {
                return RedirectToAction("MemberLogin", "Login");
            }

            var odrders = await _user.GetUserOrdersAsync(memberID);


            if (odrders == null)
            {
                return NotFound();
            }

            return View(odrders);
        }

        // 訂單詳細資料
        public async Task<IActionResult> UserDetail(string id)
        {
            if (id == null) return BadRequest();

            
            var vm= await _user.GetUserOrderDetailAsync(id);


            if(vm == null) return NotFound();


            return View(vm);
        }



        // 前台會員專區：使用者主動取消訂單 (只能取消「待付款」的訂單)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserCancelOrder(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("訂單編號不可為空");

            // 1. 取得當前登入的會員 ID (假設你存在 Session 或 Claims 中)
            var memberID = _userAccessorService.GetMemberId();

            if (memberID == null)
            {
                return RedirectToAction("MemberLogin", "Login");
            }



            // 2. 抓取訂單，並確保該訂單屬於此會員，且處於「待付款」狀態
            // 假設 'P' 代表待付款 (Pending Payment)
            var order = await _context.Order
                .Include(o => o.Tickets)
                .FirstOrDefaultAsync(o => o.OrderID == id && o.MemberID == memberID);

            if (order == null) return NotFound();

            // 🚩 安全檢查：只有狀態為待付款時才能由使用者主動取消
            if (order.OrderStatusID != "P")
            {
                TempData["Error"] = "訂單狀態已變更，無法手動取消。";
                return RedirectToAction("Details", new { id = order.OrderID });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 3. 執行取消動作
                order.OrderStatusID = "C"; // 變更訂單狀態

                // 4. 直接利用 Include 進來的 Tickets 集合進行修改
                // 這樣 EF 的 Change Tracker 會記錄所有變更，並在最後一次提交
                foreach (var ticket in order.Tickets)
                {
                    ticket.TicketsStatusID = "C";
                }

                // 5. 🚩 這裡會一次發送所有的 UPDATE (包含 Order 和所有 Tickets)
                // 這也會正確觸發 SQL Trigger 來回補庫存
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _queueService.ReleaseQueueSlot();
                TempData["Success"] = "訂單已成功取消，名額已釋出。";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // 🚩 建議：你可以用 Debug 模式看這裡的 ex.Message 是不是 SQL Trigger 報錯？
                TempData["Error"] = "取消失敗：" + ex.Message;
            }

            return RedirectToAction("UserIndex"); // 返回會員的訂單列表
        }

    }
}
