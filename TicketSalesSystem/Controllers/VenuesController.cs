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
    public class VenuesController : Controller
    {
        private readonly TicketsContext _context;

        public VenuesController(TicketsContext context)
        {
            _context = context;
        }

        // GET: Venues
        public async Task<IActionResult> Index()
        {
            var ticketsContext = _context.Venue.Include(v => v.Place).Include(v => v.VenueStatus);
            return View(await ticketsContext.ToListAsync());
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue
                .Include(v => v.Place)
                .Include(v => v.VenueStatus)
                .FirstOrDefaultAsync(m => m.VenueID == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create()
        {
            ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceID", "PlaceID");
            ViewData["VenueStatusID"] = new SelectList(_context.VenueStatus, "VenueStatusID", "VenueStatusID");
            return View();
        }

        // POST: Venues/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueID,VenueName,FloorName,AreaColor,RowCount,SeatCount,VenueStatusID,PlaceID")] Venue venue)
        {
            if (ModelState.IsValid)
            {
                _context.Add(venue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceID", "PlaceID", venue.PlaceID);
            ViewData["VenueStatusID"] = new SelectList(_context.VenueStatus, "VenueStatusID", "VenueStatusID", venue.VenueStatusID);
            return View(venue);
        }

        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceID", "PlaceID", venue.PlaceID);
            ViewData["VenueStatusID"] = new SelectList(_context.VenueStatus, "VenueStatusID", "VenueStatusID", venue.VenueStatusID);
            return View(venue);
        }

        // POST: Venues/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("VenueID,VenueName,FloorName,AreaColor,RowCount,SeatCount,VenueStatusID,PlaceID")] Venue venue)
        {
            if (id != venue.VenueID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueID))
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
            ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceID", "PlaceID", venue.PlaceID);
            ViewData["VenueStatusID"] = new SelectList(_context.VenueStatus, "VenueStatusID", "VenueStatusID", venue.VenueStatusID);
            return View(venue);
        }

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue
                .Include(v => v.Place)
                .Include(v => v.VenueStatus)
                .FirstOrDefaultAsync(m => m.VenueID == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var venue = await _context.Venue.FindAsync(id);
            if (venue != null)
            {
                _context.Venue.Remove(venue);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(string id)
        {
            return _context.Venue.Any(e => e.VenueID == id);
        }
    }
}
