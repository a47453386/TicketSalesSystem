using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.ID;

namespace TicketSalesSystem.Controllers
{
    public class FAQsController : Controller
    {
        private readonly TicketsContext _context;
        private readonly IIDService _idService;

        public FAQsController(TicketsContext context, IIDService idService)
        {
            _context = context;
            _idService = idService;
        }

        // GET: FAQs
        public async Task<IActionResult> Index()
        {
            var ticketsContext = _context.FAQ
                .Include(f => f.Employee)
                .Include(f => f.FAQPublishStatus)
                .Include(f => f.FAQType)
                .OrderBy(f=>f.FAQPublishStatus);
            return View(await ticketsContext.ToListAsync());
        }

        // GET: FAQs/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if(!FAQExists(id)) return NotFound();

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

        

        [HttpPost]
        public async Task<IActionResult> CreateTypeQuickly(string typeName)
        {
            if (typeName == null)
            {
                return Json(new { success = false, message = "名稱不能為空" });
            }

            try
            {
                string ftI = await _idService.GetNextFAQTypeID();
                var newType = new FAQType
                {
                    FAQTypeID = ftI,
                    FAQTypeName = typeName
                };
                _context.FAQType.Add(newType);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "新增成功", faqTypeId = ftI, faqTypeName = typeName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "新增失敗，可能是編號已達上限(F9)" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> EditTypeQuickly(string typeId, string typeName)
        {
            var faqType = await _context.FAQType.FindAsync(typeId);
            if (faqType == null) return Json(new { success = false, message = "找不到該分類" });

            faqType.FAQTypeName = typeName;
            _context.Update(faqType);
            await _context.SaveChangesAsync();

            return Json(new{success = true, faqTypeId = faqType.FAQTypeID,faqTypeName = faqType.FAQTypeName});
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
        public async Task<IActionResult> Create(FAQ fAQ)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(fAQ);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                PopulateDropdownLists(fAQ);
                return View(fAQ);
            }
            catch (Exception ex)
            {
                var fullMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"更新失敗: {fullMessage}");
            }
        }








        // GET: FAQs/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (!FAQExists(id)) return NotFound();

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
        public async Task<IActionResult> Edit(string id, FAQ fAQ)
        {
            if (id != fAQ.FAQID)
            {
                return NotFound();
            }

            // 移除不需要驗證的導覽屬性錯誤
            // 有時候系統會抱怨 FAQType 或 FAQPublishStatus 是必填的
            ModelState.Remove("FAQType");
            ModelState.Remove("FAQPublishStatus");
            ModelState.Remove("Employee");

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

            PopulateDropdownLists(fAQ);

            return View(fAQ);
        }










        
        // POST: FAQs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
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
