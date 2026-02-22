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

        // GET: PublicNotices1
        public async Task<IActionResult> Index()
        {
            var ticketsContext = _context.PublicNotice.Include(p => p.Employee);
            return View(await ticketsContext.ToListAsync());
        }


        public IActionResult UserIndex()
        {
            var publicNotices = _context.PublicNotice
                .Where(p => p.PublicNoticeStatus == true)
                .ToList();
            return View(publicNotices);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (string.IsNullOrEmpty(id)) return Json(new { success = false, message = "無效的 ID" });

            var notice =await _context.PublicNotice.FindAsync(id);
            if (notice == null) return Json(new { success = false, message = "找不到該公告" });

            notice.PublicNoticeStatus=!notice.PublicNoticeStatus;
            notice.UpdatedAt = DateTime.Now;

            try
            {
                _context.Update(notice);
                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    newStatus = notice.PublicNoticeStatus,
                    message = notice.PublicNoticeStatus ? "公告已發佈" : "公告已下架"
                });
            }
            catch (Exception ex) 
            {
                return Json(new { success = false, message = "更新失敗" });
            }
        }
        



        // GET: PublicNotices1/Details/5
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


        public async Task<IActionResult> UserDetails(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publicNotices = await _context.PublicNotice
               .Where(p => p.PublicNoticeStatus == true)
               .FirstOrDefaultAsync(m => m.PublicNoticeID == id);

            if (publicNotices == null)
            {
                return NotFound();
            }

            return View(publicNotices);
        }







        // GET: PublicNotices1/Create
        public IActionResult Create()
        {
           
            return View();
        }

        // POST: PublicNotices1/Create
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( PublicNotice publicNotice)
        {
            ModelState.Remove("EmployeeID");

            publicNotice.CreatedTime = DateTime.Now;
            publicNotice.UpdatedAt = null;
            publicNotice.PublicNoticeStatus = false;
            publicNotice.EmployeeID = "A23025";

            if (ModelState.IsValid)
            {
                _context.Add(publicNotice);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }            
            return View(publicNotice);
        }

        // GET: PublicNotices1/Edit/5
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

        // POST: PublicNotices1/Edit/5
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("PublicNoticeID,PublicNoticeTitle,PublicNoticeDescription,CreatedTime,UpdatedAt,RemovalTime,PublishTime,PublicNoticeStatus,EmployeeID")] PublicNotice publicNotice)
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

       

        // POST: PublicNotices1/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id) 
        {
            try
            {
                // 1. 抓出公告資料
                var publicNotice = await _context.PublicNotice.FindAsync(id);

                if (publicNotice == null)
                {
                    return Json(new { success = false, message = "找不到該公告資料。" });
                }

                // 2. 執行刪除
                _context.PublicNotice.Remove(publicNotice);
                await _context.SaveChangesAsync();

                // 3. 因為沒有圖片，直接回傳成功 JSON 即可
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // 捕捉可能的錯誤（例如資料庫連線問題）
                return Json(new { success = false, message = "刪除失敗：" + ex.Message });
            }
        }

        private bool PublicNoticeExists(string id)
        {
            return _context.PublicNotice.Any(e => e.PublicNoticeID == id);
        }
    }
}
