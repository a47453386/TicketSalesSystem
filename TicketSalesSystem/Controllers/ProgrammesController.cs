using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel;

namespace TicketSalesSystem.Controllers
{
    public class ProgrammesController : Controller
    {
        private readonly TicketsContext _context;

        public ProgrammesController(TicketsContext context)
        {
            _context = context;
        }

        // GET: Programmes
        public async Task<IActionResult> Index()
        {
            
            var programme = await _context.Programme 
                .Select(p => new VMProgramme
                {                    
                    ProgrammeID = p.ProgrammeID,
                    CoverImage = p.CoverImage,
                    ProgrammeName = p.ProgrammeName,                                      
                    PlaceName= p.Place.PlaceName,
                    StartTime = p.Session.FirstOrDefault().StartTime,
                    ProgrammeStatusName = p.ProgrammeStatus.ProgrammeStatusName,
                }).ToListAsync();

            return View(programme);
        }

        // GET: Programmes/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var programme = await _context.Programme
                .Include(p => p.Employee)
                .Include(p => p.Place)
                .Include(p => p.ProgrammeStatus)
                .FirstOrDefaultAsync(m => m.ProgrammeID == id);
            if (programme == null)
            {
                return NotFound();
            }

            return View(programme);
        }

        // GET: Programmes/Create
        public IActionResult Create()
        {
            ViewData["EmployeeID"] = new SelectList(_context.Employee, "EmployeeID", "EmployeeID");
            ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceID", "PlaceID");
            ViewData["ProgrammeStatusID"] = new SelectList(_context.ProgrammeStatus, "ProgrammeStatusID", "ProgrammeStatusID");
            return View();
        }

        // POST: Programmes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProgrammeID,ProgrammeName,ProgrammeDescription,CreatedTime,UpdatedAt,CoverImage,SeatImage,LimitPerOrder,EmployeeID,PlaceID,ProgrammeStatusID")] Programme programme)
        {
            if (ModelState.IsValid)
            {
                _context.Add(programme);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeID"] = new SelectList(_context.Employee, "EmployeeID", "EmployeeID", programme.EmployeeID);
            ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceID", "PlaceID", programme.PlaceID);
            ViewData["ProgrammeStatusID"] = new SelectList(_context.ProgrammeStatus, "ProgrammeStatusID", "ProgrammeStatusID", programme.ProgrammeStatusID);
            return View(programme);
        }

        // GET: Programmes/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var programme = await _context.Programme.FindAsync(id);
            if (programme == null)
            {
                return NotFound();
            }
            ViewData["EmployeeID"] = new SelectList(_context.Employee, "EmployeeID", "EmployeeID", programme.EmployeeID);
            ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceID", "PlaceID", programme.PlaceID);
            ViewData["ProgrammeStatusID"] = new SelectList(_context.ProgrammeStatus, "ProgrammeStatusID", "ProgrammeStatusID", programme.ProgrammeStatusID);
            return View(programme);
        }

        // POST: Programmes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ProgrammeID,ProgrammeName,ProgrammeDescription,CreatedTime,UpdatedAt,CoverImage,SeatImage,LimitPerOrder,EmployeeID,PlaceID,ProgrammeStatusID")] Programme programme)
        {
            if (id != programme.ProgrammeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(programme);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProgrammeExists(programme.ProgrammeID))
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
            ViewData["EmployeeID"] = new SelectList(_context.Employee, "EmployeeID", "EmployeeID", programme.EmployeeID);
            ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceID", "PlaceID", programme.PlaceID);
            ViewData["ProgrammeStatusID"] = new SelectList(_context.ProgrammeStatus, "ProgrammeStatusID", "ProgrammeStatusID", programme.ProgrammeStatusID);
            return View(programme);
        }

        // GET: Programmes/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var programme = await _context.Programme
                .Include(p => p.Employee)
                .Include(p => p.Place)
                .Include(p => p.ProgrammeStatus)
                .FirstOrDefaultAsync(m => m.ProgrammeID == id);
            if (programme == null)
            {
                return NotFound();
            }

            return View(programme);
        }

        // POST: Programmes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var programme = await _context.Programme.FindAsync(id);
            if (programme != null)
            {
                _context.Programme.Remove(programme);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProgrammeExists(string id)
        {
            return _context.Programme.Any(e => e.ProgrammeID == id);
        }
    }
}
