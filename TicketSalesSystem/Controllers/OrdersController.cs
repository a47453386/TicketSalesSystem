using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel;
using TicketSalesSystem.ViewModel.Booking;
using TicketSalesSystem.ViewModel.Order;

namespace TicketSalesSystem.Controllers
{
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

        // 前台會員專區的訂單列表
        public async Task<IActionResult> UserIndex()
        {
            string currentMemberID = "004bc90a-26fb-48e9-a762-653a232d86e2";

            var time=(int)Math.Max(0,(DateTime.Now.AddMinutes(10)-DateTime.Now).TotalSeconds);
            
            var odrders= await _context.Order
                .AsNoTracking()
                .Include(o=>o.OrderStatus)                
                .Include(o => o.Session).ThenInclude(s => s.Programme).ThenInclude(s => s.Place)
                .Include(o => o.Tickets).ThenInclude(t => t.TicketsArea)
                .Where(o => o.MemberID == currentMemberID)
                .OrderByDescending(o => o.OrderCreatedTime)
                .Select(o=>new VMBookingDetailsResponse
                {
                    OrderID = o.OrderID,
                    ProgrammeName = o.Session.Programme.ProgrammeName,
                    StartTime = o.Session.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    PlaceName = o.Session.Programme.Place.PlaceName,
                    FinalAmount= o.Tickets.Sum(t => t.TicketsArea.Price),
                    OrderStatusID= o.OrderStatusID,
                    OrderStatusName = o.OrderStatus.OrderStatusName,
                    Seats = o.Tickets.Select(t => $"{t.RowIndex}排{t.SeatIndex}號").ToList(),
                    //只有在狀態是待付款 (P) 時才需要計算，已完成 (Y) 的話就給 0
                    RemainingSeconds = o.OrderStatusID=="P"? time:0,
                    Success=o.OrderStatusID =="Y",
                    Message=o.OrderStatusID=="Y"?"付款完成": o.OrderStatusID == "N" ? "訂單失效" : "待付款",
                    TicketDetails = o.Tickets.Select(t => new VMTicketDetail
                    {
                        SeatInfo = $"{t.TicketsArea.TicketsAreaName} {t.RowIndex}排 {t.SeatIndex}號",
                        StatusName = t.TicketsStatus.TicketsStatusName ?? "未知",
                        Price = t.TicketsArea.Price
                    }).ToList()
                })
                .ToArrayAsync();


            if (odrders == null)
            {
                return NotFound();
            }

            return View(odrders);
        }

        // 前台會員專區的訂單詳細資料
        public async Task<IActionResult> UserDetail(string id)
        {
            if (id==null)return BadRequest();
            var order=await _context.Order
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
                    ProgrammeName= order.Session.Programme.ProgrammeName,
                    StartTime = order.Session.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    PlaceName = order.Session.Programme.Place.PlaceName,
                    FinalAmount = t.TicketsArea.Price,
                    TicketsID = t.TicketsID,
                    TicketsAreaName= t.TicketsArea.TicketsAreaName,
                    Seat= $"{t.RowIndex}排{t.SeatIndex}號",
                    CheckInCode=t.CheckInCode /*isPrintable? t.CheckInCode: null*/
                }).ToList()
            };

            return View(vm);
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
            ViewData["OrderStatusID"] = new SelectList(_context.OrderStatus, "OrderStatusID", "StatusName", order.OrderStatusID);
            ViewData["PaymentMethodID"] = new SelectList(_context.PaymentMethod, "PaymentMethodID", "MethodName", order.PaymentMethodID);
            return View(order);
        }





        // 前台會員專區：使用者主動取消訂單 (只能取消「待付款」的訂單)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserCancelOrder(string id)
        {
            // 1. 取得當前登入的會員 ID (假設你存在 Session 或 Claims 中)
            //var currentMemberId = HttpContext.Session.GetString("MemberID");
            //if (string.IsNullOrEmpty(currentMemberId)) return RedirectToAction("Login", "Member");
            var currentMemberId= "004bc90a-26fb-48e9-a762-653a232d86e2";



            // 2. 抓取訂單，並確保該訂單屬於此會員，且處於「待付款」狀態
            // 假設 'P' 代表待付款 (Pending Payment)
            var order = await _context.Order
                .Include(o => o.Tickets)
                .FirstOrDefaultAsync(o => o.OrderID == id && o.MemberID == currentMemberId);

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

                TempData["Success"] = "訂單已成功取消，名額已釋出。";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "取消失敗，請稍後再試。";
            }

            return RedirectToAction("UserIndex"); // 返回會員的訂單列表
        }


        private bool OrderExists(string id)
        {
            return _context.Order.Any(e => e.OrderID == id);
        }
    }
}
