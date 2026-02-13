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
    public class PublicNoticesController : Controller
    {
        private readonly TicketsContext _context;

        public PublicNoticesController(TicketsContext context)
        {
            _context = context;
        }

        // GET: PublicNotices
        public async Task<IActionResult> Index()
        {
            var ticketsContext = _context.PublicNotice.Include(p => p.Employee);
            return View(await ticketsContext.ToListAsync());
        }

        // GET: PublicNotices/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publicNotice = await _context.PublicNotice
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(m => m.PublicNoticeID == id);
            if (publicNotice == null)
            {
                return NotFound();
            }

            return View(publicNotice);
        }

        // GET: PublicNotices/Create
        public IActionResult Create()
        {
            ViewData["EmployeeID"] = new SelectList(_context.Employee, "EmployeeID", "EmployeeID");
            return View();
        }

        // POST: PublicNotices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PublicNoticeID,PublicNoticeTitle,PublicNoticeDescription,CreatedTime,UpdatedAt,PublicNoticeStatus,EmployeeID")] PublicNotice publicNotice)
        {
            if (ModelState.IsValid)
            {
                _context.Add(publicNotice);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeID"] = new SelectList(_context.Employee, "EmployeeID", "EmployeeID", publicNotice.EmployeeID);
            return View(publicNotice);
        }

        // GET: PublicNotices/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publicNotice = await _context.PublicNotice.FindAsync(id);
            if (publicNotice == null)
            {
                return NotFound();
            }
            ViewData["EmployeeID"] = new SelectList(_context.Employee, "EmployeeID", "EmployeeID", publicNotice.EmployeeID);
            return View(publicNotice);
        }

        // POST: PublicNotices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("PublicNoticeID,PublicNoticeTitle,PublicNoticeDescription,CreatedTime,UpdatedAt,PublicNoticeStatus,EmployeeID")] PublicNotice publicNotice)
        {
            if (id != publicNotice.PublicNoticeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(publicNotice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PublicNoticeExists(publicNotice.PublicNoticeID))
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
            ViewData["EmployeeID"] = new SelectList(_context.Employee, "EmployeeID", "EmployeeID", publicNotice.EmployeeID);
            return View(publicNotice);
        }

        // GET: PublicNotices/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publicNotice = await _context.PublicNotice
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(m => m.PublicNoticeID == id);
            if (publicNotice == null)
            {
                return NotFound();
            }

            return View(publicNotice);
        }

        // POST: PublicNotices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var publicNotice = await _context.PublicNotice.FindAsync(id);
            if (publicNotice != null)
            {
                _context.PublicNotice.Remove(publicNotice);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PublicNoticeExists(string id)
        {
            return _context.PublicNotice.Any(e => e.PublicNoticeID == id);
        }
    }
}
