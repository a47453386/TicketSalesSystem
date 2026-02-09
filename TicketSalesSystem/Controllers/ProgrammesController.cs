using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.Images;
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
                    // 🚩 修正 1：加上問號，如果 Place 是 null，則 PlaceName 為 null
                    PlaceName = p.Place != null ? p.Place.PlaceName : "未設定地點",

                    // 🚩 修正 2：最關鍵！先拿整個 Session 物件，再判斷 StartTime
                    // 或是使用 Null-conditional operator
                    StartTime = p.Session.OrderBy(s => s.StartTime).FirstOrDefault() != null
                        ? p.Session.OrderBy(s => s.StartTime).FirstOrDefault().StartTime
                        : null, // 注意：VMProgramme 的 StartTime 屬性必須改為 DateTime?// 注意：VMProgramme 的 StartTime 屬性必須改為 DateTime?

                    ProgrammeStatusName = p.ProgrammeStatus != null ? p.ProgrammeStatus.ProgrammeStatusName : "未知狀態",
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



        private void PopulateDropdownLists(Programme p = null)
        {
            ViewData["EmployeeID"] = new SelectList(_context.Employee.ToList(), "EmployeeID", "Name", p?.EmployeeID);
            ViewData["PlaceID"] = new SelectList(_context.Place.ToList(), "PlaceID", "PlaceName", p?.PlaceID);
            ViewData["ProgrammeStatusID"] = new SelectList(_context.ProgrammeStatus.ToList(), "ProgrammeStatusID", "ProgrammeStatusName", p?.ProgrammeStatusID);
        }


        // GET: Programmes/Create
        public IActionResult Create()
        {
            PopulateDropdownLists();
            return View();
        }

        // POST: Programmes/Create      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Programme programme, IFormFile newCoverImage, IFormFile newSeatImage,DateTime OnShelfTime)
        {
            //手動檢查檔案（因為我們在下個步驟會移除 Model 的圖片驗證）
            if (newCoverImage == null) ModelState.AddModelError("newCoverImage", "請選擇封面圖");
            if (newSeatImage == null) ModelState.AddModelError("newSeatImage", "請選擇座位圖");

            // 2. 關鍵：移除 ModelState 中對字串欄位的驗證(要先在資料庫設定)
            // 因為這兩個欄位在 SaveChanges 之前一定是空的，會導致 IsValid 永遠為 false
            ModelState.Remove("ProgrammeID"); // ID 是自動生成的
            ModelState.Remove("CoverImage");  // 檔名是存檔後才產生的
            ModelState.Remove("SeatImage");   // 檔名是存檔後才產生的
            ModelState.Remove("CreatedTime"); // 時間在下面會給
            ModelState.Remove("Employee");     // 導覽屬性不需驗證
            ModelState.Remove("Place");
            ModelState.Remove("ProgrammeStatus");





            if (ModelState.IsValid)
            {
                _context.Add(programme);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateDropdownLists(programme);
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
