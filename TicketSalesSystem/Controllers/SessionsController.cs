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
    public class SessionsController : Controller
    {
        private readonly TicketsContext _context;

        public SessionsController(TicketsContext context)
        {
            _context = context;
        }

        // GET: Sessions
        public async Task<IActionResult> Index()
        {
            var ticketsContext = _context.Session.Include(s => s.Programme);
            return View(await ticketsContext.ToListAsync());
        }

        // GET: Sessions/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Session
                .Include(s => s.Programme)
                .FirstOrDefaultAsync(m => m.SessionID == id);
            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }

        // GET: Sessions/Create
        public IActionResult Create()
        {
            ViewData["ProgrammeID"] = new SelectList(_context.Programme, "ProgrammeID", "ProgrammeID");
            return View();
        }

        // POST: Sessions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SessionID,SaleStartTime,SaleEndTime,StartTime,ProgrammeID")] Session session)
        {
            if (ModelState.IsValid)
            {
                _context.Add(session);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProgrammeID"] = new SelectList(_context.Programme, "ProgrammeID", "ProgrammeID", session.ProgrammeID);
            return View(session);
        }

        // GET: Sessions/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Session.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }
            ViewData["ProgrammeID"] = new SelectList(_context.Programme, "ProgrammeID", "ProgrammeID", session.ProgrammeID);
            return View(session);
        }

        // POST: Sessions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("SessionID,SaleStartTime,SaleEndTime,StartTime,ProgrammeID")] Session session)
        {
            if (id != session.SessionID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(session);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SessionExists(session.SessionID))
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
            ViewData["ProgrammeID"] = new SelectList(_context.Programme, "ProgrammeID", "ProgrammeID", session.ProgrammeID);
            return View(session);
        }

        // GET: Sessions/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Session
                .Include(s => s.Programme)
                .FirstOrDefaultAsync(m => m.SessionID == id);
            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }

        // POST: Sessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var session = await _context.Session.FindAsync(id);
            if (session != null)
            {
                _context.Session.Remove(session);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SessionExists(string id)
        {
            return _context.Session.Any(e => e.SessionID == id);
        }
    }
}
