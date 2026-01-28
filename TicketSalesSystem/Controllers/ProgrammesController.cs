using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketSalesSystem.Models;

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
            var ticketsContext = _context.Programme.Include(p => p.Employee).Include(p => p.Place).Include(p => p.ProgrammeStatus);
            return View(await ticketsContext.ToListAsync());
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
            PopulateDropdownLists();
            return View();
        }

        // POST: Programmes/Create       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProgrammeID,ProgrammeName,ProgrammeDescription,CreatedTime,UpdatedAt,CoverImage,SeatImage,LimitPerOrder,EmployeeID,PlaceID,ProgrammeStatusID")] Programme programme, IFormFile newCoverImage, IFormFile newSeatImage)
        {
            //手動檢查檔案（因為我們在下個步驟會移除 Model 的圖片驗證）
            if (newCoverImage == null) ModelState.AddModelError("newCoverImage", "請選擇封面圖");
            if (newSeatImage == null) ModelState.AddModelError("newSeatImage", "請選擇座位圖");

            // 2. 關鍵：移除 ModelState 中對字串欄位的驗證
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
                try
                {
                    // --- 呼叫預存程序：這裡對應你 Model 的變數 ---
                    var createdEntry = _context.Programme
                        .FromSqlRaw("EXEC [dbo].[CreateProgramme] {0}, {1}, {2}, {3}, {4}, {5}",
                            programme.ProgrammeName,           // {0}
                            programme.ProgrammeDescription, // {1}
                            programme.LimitPerOrder,           // {2}
                            programme.EmployeeID,              // {3}
                            programme.PlaceID,                  // {4}
                            programme.ProgrammeStatusID)        // {5}
                        .AsEnumerable()
                        .FirstOrDefault();

                    if (createdEntry != null)
                    {
                        string cleanID = createdEntry.ProgrammeID.Trim();
                        bool needsUpdate = false;

                        // --- 圖片變數處理與存檔 ---
                        if (newCoverImage != null && newCoverImage.Length > 0)
                        {
                            // 檔名規則：C + PRG00001 + .jpg (長度約 12~13 字元)
                            string fileName = "C" + cleanID + Path.GetExtension(newCoverImage.FileName);
                            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Photos/CoverImage", fileName);
                            using (var stream = new FileStream(path, FileMode.Create)) { await newCoverImage.CopyToAsync(stream); }

                            createdEntry.CoverImage = fileName;
                            needsUpdate = true;
                        }

                        if (newSeatImage != null && newSeatImage.Length > 0)
                        {
                            string fileName = "S" + cleanID + Path.GetExtension(newSeatImage.FileName);
                            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Photos/SeatImage", fileName);
                            using (var stream = new FileStream(path, FileMode.Create)) { await newSeatImage.CopyToAsync(stream); }

                            createdEntry.SeatImage = fileName;
                            needsUpdate = true;
                        }

                        // --- 更新圖片檔名回資料庫 ---
                        if (needsUpdate)
                        {
                            createdEntry.UpdatedAt = DateTime.Now;
                            _context.Update(createdEntry);
                            await _context.SaveChangesAsync();
                        }

                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "資料庫寫入失敗：" + ex.Message);
                }
            }

            // 失敗則回填選單
            PopulateDropdownLists(programme);
            return View(programme);
        }

        private void PopulateDropdownLists(Programme p = null)
        {
            ViewData["EmployeeID"] = new SelectList(_context.Employee.ToList(), "EmployeeID", "Name", p?.EmployeeID);
            ViewData["PlaceID"] = new SelectList(_context.Place.ToList(), "PlaceID", "PlaceName", p?.PlaceID);
            ViewData["ProgrammeStatusID"] = new SelectList(_context.ProgrammeStatus.ToList(), "ProgrammeStatusID", "ProgrammeStatusName", p?.ProgrammeStatusID);
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
