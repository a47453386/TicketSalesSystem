using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.Service.Queue;
using TicketSalesSystem.ViewModel;
using TicketSalesSystem.ViewModel.Booking;
using TicketSalesSystem.ViewModel.Order;

namespace TicketSalesSystem.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(AuthenticationSchemes = "EmployeeScheme", Roles = "S,A,D")]
    public class OrdersController : Controller
    {
        private readonly TicketsContext _context;        
       
        public OrdersController(TicketsContext context)
        {
            _context = context;                    
        }
                
        // 後台訂單管理頁面
        public async Task<IActionResult> Index(string searchString, string statusFilter)
        {
            // 🚩 1. 基礎查詢，包含必要關聯
            var orders = _context.Order
                .Include(o => o.Member)
                .Include(o => o.OrderStatus)
                .Include(o => o.Question) // 為了判斷是否有退票申請
                .AsQueryable();

            // 🚩 2. 搜尋邏輯 (依訂單編號或帳號)
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(s => s.OrderID.Contains(searchString)
                                        || s.Member.MemberID.Contains(searchString));
            }

            // 🚩 3. 狀態篩選
            if (!string.IsNullOrEmpty(statusFilter))
            {
                orders = orders.Where(x => x.OrderStatusID == statusFilter);
            }

            // 排序：最新的訂單排前面
            var result = await orders.OrderByDescending(o => o.OrderCreatedTime).ToListAsync();

            // 準備篩選用的下拉選單
            ViewBag.StatusList = new SelectList(_context.OrderStatus, "OrderStatusID", "OrderStatusName");

            return View(result);
        }

        
       
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var order = await _context.Order
                .Include(o => o.Member)
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentMethod)
                .Include(o => o.Tickets)
                    .ThenInclude(t => t.TicketsArea)
                .Include(o => o.Tickets)
                    .ThenInclude(t => t.Session)
                        .ThenInclude(s => s.Programme)
                .Include(o => o.Question) // 包含客服提問紀錄
                .FirstOrDefaultAsync(m => m.OrderID == id);

            if (order == null) return NotFound();

            return View(order);
        }


        
        //訂單詳細資料頁面 (後台)
        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
             .Include(o => o.Member)
                .ThenInclude(t => t.MemberLogin)
             .Include(o => o.PaymentMethod) // 為了顯示付款方式名稱
             .Include(o => o.OrderStatus)
             .Include(o => o.Question)
             .Include(o => o.Tickets)
                 .ThenInclude(t => t.Session)
                     .ThenInclude(s => s.Programme) // 為了抓活動名稱
             .Include(o => o.Tickets)
                 .ThenInclude(t => t.TicketsArea) // 為了抓票區
             .Include(o => o.Tickets)
                 .ThenInclude(t => t.TicketsStatus) // 為了抓狀態名稱
             .FirstOrDefaultAsync(m => m.OrderID == id);

            if (order == null)
            {
                return NotFound();
            }
           
            ViewData["OrderStatusID"] = new SelectList(_context.OrderStatus, "OrderStatusID", "OrderStatusName", order.OrderStatusID);
            ViewData["PaymentMethodID"] = new SelectList(_context.PaymentMethod, "PaymentMethodID", "PaymentMethodName", order.PaymentMethodID);
           
            return View(order);
        }

        // POST: Orders/Edit/5        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id,Order order)
        {
            if (id != order.OrderID) return NotFound();
            

            if (ModelState.IsValid)
            {
                
                
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    //取得資料庫中的「原始」資料 (用於比對狀態)
                    var originalOrder = await _context.Order
                    .AsNoTracking()
                    .Include(o => o.Tickets)
                    .FirstOrDefaultAsync(o => o.OrderID == id);

                    if (originalOrder == null) return NotFound();

                    // 偵測狀態是否變更為 'C' (取消/退票)
                    var cancelStatuses = new[] { "C", "N" };
                    if (!cancelStatuses.Contains(originalOrder.OrderStatusID) && cancelStatuses.Contains(order.OrderStatusID))
                    {
                        // 抓出該訂單的所有票券，全部設為 'C'
                        await _context.Tickets
                             .Where(t => t.OrderID == id)
                             .ExecuteUpdateAsync(s => s.SetProperty(t => t.TicketsStatusID, order.OrderStatusID));
                    }
                    // 更新訂單主體
                    _context.Update(order);

                    //處理相關的 Question
                    var relatedQuestions = await _context.Question
                        .Include(q => q.Reply) // 務必包含 Reply 列表
                        .Where(q => q.OrderID == id && q.OrderID != null)
                        .ToListAsync();

                    foreach (var q in relatedQuestions)
                    {
                        // 方案：新增一筆「系統自動回覆」來改變狀態
                        var systemReply = new Reply
                        {
                            ReplyID = Guid.NewGuid().ToString(),
                            QuestionID = q.QuestionID,
                            ReplyDescription = "【系統自動訊息】管理員已核准退票並取消訂單，此提問已自動結案。",
                            CreatedTime = DateTime.Now,
                            ReplyStatusID = "Y", //已回覆
                        };

                        _context.Reply.Add(systemReply);
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Index));

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderID)) return NotFound();
                    else throw;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "更新失敗：" + ex.Message);
                }
            
            }
            // 若失敗，重新準備下拉選單並回傳頁面
            ViewData["OrderStatusID"] = new SelectList(_context.OrderStatus, "OrderStatusID", "OrderStatusName", order.OrderStatusID);
            ViewData["PaymentMethodID"] = new SelectList(_context.PaymentMethod, "PaymentMethodID", "PaymentMethodName", order.PaymentMethodID);
            return View(order);
        }





        private bool OrderExists(string id)
        {
            return _context.Order.Any(e => e.OrderID == id);
        }
    }
}
