using Microsoft.AspNetCore.Authorization;
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
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(AuthenticationSchemes = "EmployeeScheme", Roles = "S,A,B")]
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
            var faqs = await _context.FAQ
                .Include(f => f.Employee)
                .Include(f => f.FAQPublishStatus)
                .Include(f => f.FAQType)
                .OrderBy(f=>f.FAQPublishStatusID)
                .ToListAsync();
            return View(faqs);
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
        // 🚩 [POST] 快速新增 FAQ 狀態 (ID 自行輸入)
        [HttpPost]
        public async Task<IActionResult> QuickCreateStatus(string id, string name)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
                return Json(new { success = false, message = "代碼與名稱均不能為空" });

            // 檢查 ID 是否重複，因為是手動輸入
            if (await _context.FAQPublishStatus.AnyAsync(s => s.FAQPublishStatusID == id))
                return Json(new { success = false, message = "此狀態代碼已存在" });

            try
            {
                var newStatus = new FAQPublishStatus
                {
                    FAQPublishStatusID = id,
                    FAQPublishStatusName = name
                };
                _context.FAQPublishStatus.Add(newStatus);
                await _context.SaveChangesAsync();

                // 💡 這裡回傳的 name 格式為 "ID - Name"，讓前端下拉選單同步
                return Json(new { success = true, id = id, name = $"{id} - {name}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "系統錯誤：" + ex.Message });
            }
        }

        // 🚩 [POST] 快速編輯 FAQ 狀態 (ID 唯讀)
        [HttpPost]
        public async Task<IActionResult> EditStatusQuickly(string id, string name)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
                return Json(new { success = false, message = "找不到識別 ID 或名稱為空" });

            var status = await _context.FAQPublishStatus.FindAsync(id);
            if (status == null) return Json(new { success = false, message = "找不到該狀態" });

            status.FAQPublishStatusName = name;
            await _context.SaveChangesAsync();

            // 回傳更新後的 FullDisplayName 格式
            return Json(new { success = true, id = id, name = $"{id} - {name}" });
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
            ModelState.Remove("EmployeeID");
            try
            {
                if (ModelState.IsValid)
                {
                    fAQ.EmployeeID = User.Identity.Name;


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
        // POST: FAQs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                // 1. 抓出 FAQ 資料
                var fAQ = await _context.FAQ.FindAsync(id);

                if (fAQ == null)
                {
                    return Json(new { success = false, message = "找不到該常見問題資料。" });
                }

                // 2. 執行刪除
                _context.FAQ.Remove(fAQ);
                await _context.SaveChangesAsync();

                // 🚩 3. 回傳 JSON 成功訊號，觸發前端的 SweetAlert 成功視窗
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // 捕捉可能的資料庫錯誤
                return Json(new { success = false, message = "刪除失敗，原因：" + ex.Message });
            }
        }

        private bool FAQExists(string id)
        {
            return _context.FAQ.Any(e => e.FAQID == id);
        }



        private void PopulateDropdownLists(FAQ f = null)
        {
            ViewData["EmployeeID"] = new SelectList(_context.Employee.ToList(), "EmployeeID", "Name", f?.EmployeeID);
            ViewData["FAQPublishStatusID"] = new SelectList(_context.FAQPublishStatus.ToList(),"FAQPublishStatusID", "FullDisplayName",f?.FAQPublishStatusID);
            ViewData["FAQTypeID"] = new SelectList(_context.FAQType.ToList(), "FAQTypeID", "FAQTypeName", f?.FAQTypeID);
        }
    }

}
