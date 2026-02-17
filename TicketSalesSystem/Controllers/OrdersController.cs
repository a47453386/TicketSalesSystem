using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketSalesSystem.Models;
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

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var ticketsContext = _context.Order.Include(o => o.Member).Include(o => o.OrderStatus).Include(o => o.PaymentMethod).Include(o => o.Session);
            return View(await ticketsContext.ToListAsync());
        }

        
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
                .Select(o=>new VMBookingResponse
                {
                    OrderID = o.OrderID,
                    ProgrammeName = o.Session.Programme.ProgrammeName,
                    StartTime = o.Session.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    PlaceName = o.Session.Programme.Place.PlaceName,
                    FinalAmount= o.Tickets.Sum(t => t.TicketsArea.Price),
                    OrderStatusName = o.OrderStatus.OrderStatusName,
                    Seats = o.Tickets.Select(t => $"{t.RowIndex}排{t.SeatIndex}號").ToList(),
                    //只有在狀態是待付款 (P) 時才需要計算，已完成 (Y) 的話就給 0
                    RemainingSeconds = o.OrderStatusID=="P"? time:0,
                    Success=o.OrderStatusID =="Y",
                    Message=o.OrderStatusID=="Y"?"付款完成": o.OrderStatusID == "N" ? "訂單失效" : "待付款",
                    
                })
                .ToArrayAsync();


            if (odrders == null)
            {
                return NotFound();
            }

            return View(odrders);
        }
        public async Task<IActionResult> UserDetail(string id)
        {
            if (id==null)return BadRequest();
            var order=await _context.Order
                .AsNoTracking()
                .Include(o => o.OrderStatus)
                .Include(o => o.Session).ThenInclude(s => s.Programme).ThenInclude(s => s.Place)
                .Include(o => o.Tickets).ThenInclude(t => t.TicketsArea)
                .Include(o => o.Tickets).ThenInclude(t => t.Session)
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


        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["MemberID"] = new SelectList(_context.Member, "MemberID", "MemberID", order.MemberID);
            ViewData["OrderStatusID"] = new SelectList(_context.OrderStatus, "OrderStatusID", "OrderStatusID", order.OrderStatusID);
            ViewData["PaymentMethodID"] = new SelectList(_context.PaymentMethod, "PaymentMethodID", "PaymentMethodID", order.PaymentMethodID);
            ViewData["SessionID"] = new SelectList(_context.Session, "SessionID", "SessionID", order.SessionID);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("OrderID,OrderCreatedTime,PaymentTradeNO,PaymentDescription,PaymentStatus,PaidTime,MemberID,PaymentMethodID,OrderStatusID,SessionID")] Order order)
        {
            if (id != order.OrderID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MemberID"] = new SelectList(_context.Member, "MemberID", "MemberID", order.MemberID);
            ViewData["OrderStatusID"] = new SelectList(_context.OrderStatus, "OrderStatusID", "OrderStatusID", order.OrderStatusID);
            ViewData["PaymentMethodID"] = new SelectList(_context.PaymentMethod, "PaymentMethodID", "PaymentMethodID", order.PaymentMethodID);
            ViewData["SessionID"] = new SelectList(_context.Session, "SessionID", "SessionID", order.SessionID);
            return View(order);
        }


        private bool OrderExists(string id)
        {
            return _context.Order.Any(e => e.OrderID == id);
        }
    }
}
