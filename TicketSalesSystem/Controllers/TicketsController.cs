using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "MemberScheme")]
    public class TicketsController : Controller
    {
        private readonly TicketsContext _context;

        public TicketsController(TicketsContext context)
        {
            _context = context;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var ticketsContext = _context.Tickets.Include(t => t.Order).Include(t => t.Session).Include(t => t.TicketsArea).Include(t => t.TicketsStatus);
            return View(await ticketsContext.ToListAsync());
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tickets = await _context.Tickets
                .Include(t => t.Order)
                .Include(t => t.Session)
                .Include(t => t.TicketsArea)
                .Include(t => t.TicketsStatus)
                .FirstOrDefaultAsync(m => m.TicketsID == id);
            if (tickets == null)
            {
                return NotFound();
            }

            return View(tickets);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            ViewData["OrderID"] = new SelectList(_context.Order, "OrderID", "OrderID");
            ViewData["SessionID"] = new SelectList(_context.Session, "SessionID", "SessionID");
            ViewData["TicketsAreaID"] = new SelectList(_context.TicketsArea, "TicketsAreaID", "TicketsAreaID");
            ViewData["TicketsStatusID"] = new SelectList(_context.TicketsStatus, "TicketsStatusID", "TicketsStatusID");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketsID,RowIndex,SeatIndex,RefundTime,CreatedTime,ScannedTime,TicketsStatusID,OrderID,SessionID,TicketsAreaID")] Tickets tickets)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tickets);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrderID"] = new SelectList(_context.Order, "OrderID", "OrderID", tickets.OrderID);
            ViewData["SessionID"] = new SelectList(_context.Session, "SessionID", "SessionID", tickets.SessionID);
            ViewData["TicketsAreaID"] = new SelectList(_context.TicketsArea, "TicketsAreaID", "TicketsAreaID", tickets.TicketsAreaID);
            ViewData["TicketsStatusID"] = new SelectList(_context.TicketsStatus, "TicketsStatusID", "TicketsStatusID", tickets.TicketsStatusID);
            return View(tickets);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tickets = await _context.Tickets.FindAsync(id);
            if (tickets == null)
            {
                return NotFound();
            }
            ViewData["OrderID"] = new SelectList(_context.Order, "OrderID", "OrderID", tickets.OrderID);
            ViewData["SessionID"] = new SelectList(_context.Session, "SessionID", "SessionID", tickets.SessionID);
            ViewData["TicketsAreaID"] = new SelectList(_context.TicketsArea, "TicketsAreaID", "TicketsAreaID", tickets.TicketsAreaID);
            ViewData["TicketsStatusID"] = new SelectList(_context.TicketsStatus, "TicketsStatusID", "TicketsStatusID", tickets.TicketsStatusID);
            return View(tickets);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("TicketsID,RowIndex,SeatIndex,RefundTime,CreatedTime,ScannedTime,TicketsStatusID,OrderID,SessionID,TicketsAreaID")] Tickets tickets)
        {
            if (id != tickets.TicketsID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tickets);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketsExists(tickets.TicketsID))
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
            ViewData["OrderID"] = new SelectList(_context.Order, "OrderID", "OrderID", tickets.OrderID);
            ViewData["SessionID"] = new SelectList(_context.Session, "SessionID", "SessionID", tickets.SessionID);
            ViewData["TicketsAreaID"] = new SelectList(_context.TicketsArea, "TicketsAreaID", "TicketsAreaID", tickets.TicketsAreaID);
            ViewData["TicketsStatusID"] = new SelectList(_context.TicketsStatus, "TicketsStatusID", "TicketsStatusID", tickets.TicketsStatusID);
            return View(tickets);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tickets = await _context.Tickets
                .Include(t => t.Order)
                .Include(t => t.Session)
                .Include(t => t.TicketsArea)
                .Include(t => t.TicketsStatus)
                .FirstOrDefaultAsync(m => m.TicketsID == id);
            if (tickets == null)
            {
                return NotFound();
            }

            return View(tickets);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var tickets = await _context.Tickets.FindAsync(id);
            if (tickets != null)
            {
                _context.Tickets.Remove(tickets);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketsExists(string id)
        {
            return _context.Tickets.Any(e => e.TicketsID == id);
        }
    }
}
