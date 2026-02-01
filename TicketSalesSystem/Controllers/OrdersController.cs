using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;

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
            var ticketsContext = _context.Order.Include(o => o.Member).Include(o => o.OrderStatus).Include(o => o.Payment).Include(o => o.PaymentMethod).Include(o => o.Session);
            return View(await ticketsContext.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.Member)
                .Include(o => o.OrderStatus)
                .Include(o => o.Payment)
                .Include(o => o.PaymentMethod)
                .Include(o => o.Session)
                .FirstOrDefaultAsync(m => m.OrderID == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["MemberID"] = new SelectList(_context.Member, "MemberID", "MemberID");
            ViewData["OrderStatusID"] = new SelectList(_context.OrderStatus, "OrderStatusID", "OrderStatusID");
            ViewData["PaymentTradeNO"] = new SelectList(_context.Payment, "PaymentTradeNO", "PaymentTradeNO");
            ViewData["PaymentMethodID"] = new SelectList(_context.PaymentMethod, "PaymentMethodID", "PaymentMethodID");
            ViewData["SessionID"] = new SelectList(_context.Session, "SessionID", "SessionID");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderID,OrderCreatedTime,PaidTime,MemberID,PaymentMethodID,OrderStatusID,SessionID,PaymentTradeNO")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MemberID"] = new SelectList(_context.Member, "MemberID", "MemberID", order.MemberID);
            ViewData["OrderStatusID"] = new SelectList(_context.OrderStatus, "OrderStatusID", "OrderStatusID", order.OrderStatusID);
            ViewData["PaymentTradeNO"] = new SelectList(_context.Payment, "PaymentTradeNO", "PaymentTradeNO", order.PaymentTradeNO);
            ViewData["PaymentMethodID"] = new SelectList(_context.PaymentMethod, "PaymentMethodID", "PaymentMethodID", order.PaymentMethodID);
            ViewData["SessionID"] = new SelectList(_context.Session, "SessionID", "SessionID", order.SessionID);
            return View(order);
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
            ViewData["PaymentTradeNO"] = new SelectList(_context.Payment, "PaymentTradeNO", "PaymentTradeNO", order.PaymentTradeNO);
            ViewData["PaymentMethodID"] = new SelectList(_context.PaymentMethod, "PaymentMethodID", "PaymentMethodID", order.PaymentMethodID);
            ViewData["SessionID"] = new SelectList(_context.Session, "SessionID", "SessionID", order.SessionID);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("OrderID,OrderCreatedTime,PaidTime,MemberID,PaymentMethodID,OrderStatusID,SessionID,PaymentTradeNO")] Order order)
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
            ViewData["PaymentTradeNO"] = new SelectList(_context.Payment, "PaymentTradeNO", "PaymentTradeNO", order.PaymentTradeNO);
            ViewData["PaymentMethodID"] = new SelectList(_context.PaymentMethod, "PaymentMethodID", "PaymentMethodID", order.PaymentMethodID);
            ViewData["SessionID"] = new SelectList(_context.Session, "SessionID", "SessionID", order.SessionID);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.Member)
                .Include(o => o.OrderStatus)
                .Include(o => o.Payment)
                .Include(o => o.PaymentMethod)
                .Include(o => o.Session)
                .FirstOrDefaultAsync(m => m.OrderID == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order != null)
            {
                _context.Order.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(string id)
        {
            return _context.Order.Any(e => e.OrderID == id);
        }
    }
}
