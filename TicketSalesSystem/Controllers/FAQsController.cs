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
    public class FAQsController : Controller
    {
        private readonly TicketsContext _context;

        public FAQsController(TicketsContext context)
        {
            _context = context;
        }

        // GET: FAQs
        public async Task<IActionResult> Index()
        {
            var ticketsContext = _context.FAQ.Include(f => f.Employee).Include(f => f.FAQPublishStatus).Include(f => f.FAQType);
            return View(await ticketsContext.ToListAsync());
        }

        // GET: FAQs/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fAQ = await _context.FAQ
                .Include(f => f.Employee)
                .Include(f => f.FAQPublishStatus)
                .Include(f => f.FAQType)
                .FirstOrDefaultAsync(m => m.FAQID == id);
            if (fAQ == null)
            {
                return NotFound();
            }

            return View(fAQ);
        }

        // GET: FAQs/Create
        public IActionResult Create()
        {
            PopulateDropdownLists();
            return View();
        }

        // POST: FAQs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( FAQ fAQ,string typename)
        {
            if(typename==null)
            {
                return Json(new { success = false, message = "名稱不能為空" });
            }

            try
            {
                var newType=new FAQType
                {
                    FAQTypeID=Guid.NewGuid().ToString(),
                    FAQTypeName= typename
                }
            }






            if (ModelState.IsValid)
            {
                _context.Add(fAQ);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateDropdownLists(fAQ);
            return View(fAQ);
        }

        // GET: FAQs/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fAQ = await _context.FAQ.FindAsync(id);
            if (fAQ == null)
            {
                return NotFound();
            }
            PopulateDropdownLists();
            return View(fAQ);
        }

        // POST: FAQs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("FAQID,FAQTitle,FAQDescription,EmployeeID,FAQPublishStatusID,FAQTypeID")] FAQ fAQ)
        {
            if (id != fAQ.FAQID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fAQ);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FAQExists(fAQ.FAQID))
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
            ViewData["EmployeeID"] = new SelectList(_context.Employee, "EmployeeID", "EmployeeID", fAQ.EmployeeID);
            ViewData["FAQPublishStatusID"] = new SelectList(_context.FAQPublishStatus, "FAQPublishStatusID", "FAQPublishStatusID", fAQ.FAQPublishStatusID);
            ViewData["FAQTypeID"] = new SelectList(_context.FAQType, "FAQTypeID", "FAQTypeID", fAQ.FAQTypeID);
            return View(fAQ);
        }

        // GET: FAQs/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fAQ = await _context.FAQ
                .Include(f => f.Employee)
                .Include(f => f.FAQPublishStatus)
                .Include(f => f.FAQType)
                .FirstOrDefaultAsync(m => m.FAQID == id);
            if (fAQ == null)
            {
                return NotFound();
            }

            return View(fAQ);
        }

        // POST: FAQs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var fAQ = await _context.FAQ.FindAsync(id);
            if (fAQ != null)
            {
                _context.FAQ.Remove(fAQ);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FAQExists(string id)
        {
            return _context.FAQ.Any(e => e.FAQID == id);
        }



        private void PopulateDropdownLists(FAQ f = null)
        {
            ViewData["EmployeeID"] = new SelectList(_context.Employee.ToList(), "EmployeeID", "Name", f?.EmployeeID);
            ViewData["FAQPublishStatusID"] = new SelectList(_context.FAQPublishStatus.ToList(), "FAQPublishStatusID","FAQPublishStatusName",  f?.FAQPublishStatusID);
            ViewData["FAQTypeID"] = new SelectList(_context.FAQType.ToList(), "FAQTypeID", "FAQTypeName", f?.FAQTypeID);
        }
    }

}
