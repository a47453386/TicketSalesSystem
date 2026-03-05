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

        // 前台會員專區的訂單詳細資料
        public async Task<IActionResult> UserDetail(string id)
        {
            if (id == null) return BadRequest();
            var order = await _context.Order
                .AsNoTracking()
                .Include(o => o.OrderStatus)
                .Include(o => o.Session).ThenInclude(s => s.Programme).ThenInclude(s => s.Place)
                .Include(o => o.Tickets).ThenInclude(t => t.TicketsArea)
                .Include(o => o.Tickets).ThenInclude(t => t.Session)
                .Include(o => o.Tickets).ThenInclude(t => t.TicketsStatus)
                .Where(o => o.OrderID == id)
                .FirstOrDefaultAsync();
            if (order == null) return NotFound();

            var isPrintable = true;

            //判斷天數
            //DateTime.Now >= order.Session.StartTime.AddDays(-15);

            var vm = new VMUserOrderDetail
            {
                OrderID = order.OrderID,
                ProgrammeName = order.Session.Programme.ProgrammeName,
                StartTime = order.Session.StartTime.ToString("yyyy-MM-dd HH:mm"),
                PlaceName = order.Session.Programme.Place.PlaceName,
                FinalAmount = order.Tickets.Sum(t => t.TicketsArea.Price),
                OrderStatusName = order.OrderStatus.OrderStatusName,
                Seats = order.Tickets.Select(t => $"{t.RowIndex}排{t.SeatIndex}號").ToList(),
                IsPrintable = isPrintable,
                Tickets = order.Tickets.Select(t => new VMUserTicketItem
                {
                    OrderID = order.OrderID,
                    ProgrammeName = order.Session.Programme.ProgrammeName,
                    StartTime = order.Session.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    PlaceName = order.Session.Programme.Place.PlaceName,
                    FinalAmount = t.TicketsArea.Price,
                    TicketsID = t.TicketsID,
                    TicketsAreaName = t.TicketsArea.TicketsAreaName,
                    Seat = $"{t.RowIndex}排{t.SeatIndex}號",
                    CheckInCode = t.CheckInCode /*isPrintable? t.CheckInCode: null*/
                }).ToList()
            };

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
                order.OrderStatusID = "C"; // 改為取消狀態

                // 4. 連動更新票券狀態為 'C' (這會觸發你的 SQL Trigger 自動回補庫存)
                await _context.Tickets
                     .Where(t => t.OrderID == id)
                     .ExecuteUpdateAsync(s => s.SetProperty(t => t.TicketsStatusID, "C"));

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _queueService.ReleaseQueueSlot();
                TempData["Success"] = "訂單已成功取消，名額已釋出。";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "取消失敗，請稍後再試。";
            }

            return RedirectToAction("UserIndex"); // 返回會員的訂單列表
        }

    }
}
