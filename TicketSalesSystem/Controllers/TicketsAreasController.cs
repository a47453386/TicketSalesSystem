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
    public class TicketsAreasController : Controller
    {
        private readonly TicketsContext _context;

        public TicketsAreasController(TicketsContext context)
        {
            _context = context;
        }

        // GET: TicketsAreas
        public async Task<IActionResult> Index()
        {
            var ticketsContext = _context.TicketsArea.Include(t => t.TicketsAreaStatus).Include(t => t.Venue);
            return View(await ticketsContext.ToListAsync());
        }

        // GET: TicketsAreas/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketsArea = await _context.TicketsArea
                
                .Include(t => t.TicketsAreaStatus)
                .Include(t => t.Venue)
                .FirstOrDefaultAsync(m => m.TicketsAreaID == id);
            if (ticketsArea == null)
            {
                return NotFound();
            }

            return View(ticketsArea);
        }

        // GET: TicketsAreas/Create
        public IActionResult Create()
        {
            
            ViewData["TicketsAreaStatusID"] = new SelectList(_context.TicketsAreaStatus, "TicketsAreaStatusID", "TicketsAreaStatusID");
            ViewData["VenusID"] = new SelectList(_context.Venue, "VenueID", "VenueID");
            return View();
        }

        // POST: TicketsAreas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketsAreaID,TicketsAreaName,Price,TicketsAreaStatusID,VenusID")] TicketsArea ticketsArea)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticketsArea);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["TicketsAreaStatusID"] = new SelectList(_context.TicketsAreaStatus, "TicketsAreaStatusID", "TicketsAreaStatusID", ticketsArea.TicketsAreaStatusID);
            ViewData["VenusID"] = new SelectList(_context.Venue, "VenueID", "VenueID", ticketsArea.VenueID);
            return View(ticketsArea);
        }

        // GET: TicketsAreas/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketsArea = await _context.TicketsArea.FindAsync(id);
            if (ticketsArea == null)
            {
                return NotFound();
            }
           
            ViewData["TicketsAreaStatusID"] = new SelectList(_context.TicketsAreaStatus, "TicketsAreaStatusID", "TicketsAreaStatusID", ticketsArea.TicketsAreaStatusID);
            ViewData["VenusID"] = new SelectList(_context.Venue, "VenueID", "VenueID", ticketsArea.VenueID);
            return View(ticketsArea);
        }

        // POST: TicketsAreas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("TicketsAreaID,TicketsAreaName,Price,TicketsAreaStatusID,VenusID")] TicketsArea ticketsArea)
        {
            if (id != ticketsArea.TicketsAreaID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketsArea);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketsAreaExists(ticketsArea.TicketsAreaID))
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
           
            ViewData["TicketsAreaStatusID"] = new SelectList(_context.TicketsAreaStatus, "TicketsAreaStatusID", "TicketsAreaStatusID", ticketsArea.TicketsAreaStatusID);
            ViewData["VenusID"] = new SelectList(_context.Venue, "VenueID", "VenueID", ticketsArea.VenueID);
            return View(ticketsArea);
        }

        // GET: TicketsAreas/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketsArea = await _context.TicketsArea
                
                .Include(t => t.TicketsAreaStatus)
                .Include(t => t.Venue)
                .FirstOrDefaultAsync(m => m.TicketsAreaID == id);
            if (ticketsArea == null)
            {
                return NotFound();
            }

            return View(ticketsArea);
        }

        // POST: TicketsAreas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var ticketsArea = await _context.TicketsArea.FindAsync(id);
            if (ticketsArea != null)
            {
                _context.TicketsArea.Remove(ticketsArea);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketsAreaExists(string id)
        {
            return _context.TicketsArea.Any(e => e.TicketsAreaID == id);
        }
    }
}
