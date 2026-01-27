using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
            ViewData["EmployeeID"] = new SelectList(_context.Employee,"Name", "Name");
            ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceName", "PlaceName");
            ViewData["ProgrammeStatusID"] = new SelectList(_context.ProgrammeStatus, "ProgrammeStatusName", "ProgrammeStatusName");
            return View();
        }

        // POST: Programmes/Create       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProgrammeID,ProgrammeName,ProgrammeDescription,CreatedTime,UpdatedAt,CoverImage,SeatImage,LimitPerOrder,EmployeeID,PlaceID,ProgrammeStatusID")] Programme programme,IFormFile newCoverImage,IFormFile newSeatImage)
        {
            programme.CreatedTime= DateTime.Now;

            // 1. 手動檢查圖片是否為空 (如果這是必填項)
            if (newCoverImage == null) ModelState.AddModelError("CoverImage", "請上傳封面圖");
            if (newSeatImage == null) ModelState.AddModelError("SeatImage", "請上傳座位圖");

            // 2. 檢查圖片格式 (不直接存檔，只檢查)
            IFormFile[] photos = { newCoverImage, newSeatImage };
            foreach (var file in photos.Where(f => f != null))
            {
                if (file.ContentType != "image/jpeg" && file.ContentType != "image/png")
                {
                    ModelState.AddModelError("", $"檔案 {file.FileName} 格式不符");
                }
            }

            if (ModelState.IsValid)
            {
                // 3. 通過驗證後，才開始處理存檔邏輯
                for (int i = 0; i < photos.Length; i++)
                {
                    var file = photos[i];
                    string prefix = (i == 0) ? "C" : "S";
                    string fileName = prefix + programme.ProgrammeID+ Path.GetExtension(file.FileName);

                    string subFolder = (i == 0) ? "CoverImage" : "SeatImage";
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photos", subFolder, fileName);

                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // 更新 Model 的欄位值
                    if (i == 0) programme.CoverImage = fileName;
                    else programme.SeatImage = fileName;
                }

                _context.Add(programme);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeID"] = new SelectList(_context.Employee, "EmployeeID", "Name", "EmployeeID");
            ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceID", "PlaceName", "PlaceName");
            ViewData["ProgrammeStatusID"] = new SelectList(_context.ProgrammeStatus, "ProgrammeStatusID", "ProgrammeStatusName", "ProgrammeStatusName");
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
