using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.ID;
using TicketSalesSystem.Service.Images;
using TicketSalesSystem.ViewModel.Place;

namespace TicketSalesSystem.Controllers
{
    public class PlacesController : Controller
    {
        private readonly TicketsContext _context;
        private readonly IIDService _idService;
        private readonly IFileService _fileService;

        public PlacesController(TicketsContext context, IIDService idService, IFileService fileService)
        {
            _context = context;
            _idService = idService;
            _fileService = fileService;
        }

        // GET: Places
        public async Task<IActionResult> Index()
        {
            var places =await _context.Place
                .Include(p => p.Venue) 
                .ThenInclude(v => v.VenueStatus)
                .OrderBy(p => p.PlaceID)
                .ToListAsync();

            return View(places);
        }

        // GET: Places/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
           

            var place = await _context.Place
                .Include(p => p.Venue)
                .ThenInclude(v => v.VenueStatus)
                .FirstOrDefaultAsync(m => m.PlaceID == id);

            if (place == null) return NotFound();           

            return View(place);
        }


        [HttpPost]
        public async Task<IActionResult> QuickCreateStatus(string id, string name)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
                return Json(new { success = false, message = "請輸入完整資料" });

            if (await _context.VenueStatus.AnyAsync(s => s.VenueStatusID == id))
                return Json(new { success = false, message = "代碼已存在" });

            var status = new VenueStatus { VenueStatusID = id, VenueStatusName = name };
            _context.VenueStatus.Add(status);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = id, name = name });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVenueStatus(string id, string name)
        {
            var status = await _context.VenueStatus.FindAsync(id);
            if (status == null) return Json(new { success = false, message = "找不到該狀態" });

            status.VenueStatusName = name; // 修改名稱
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = id, name = name });
        }

        // GET: Places/Create
        public async Task<IActionResult> Create()
        {
            PopulateDropdownLists();
            return View();
        }

        // POST: Places/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VMPlaceCreate vm, IFormFile file)
        {
            ModelState.Remove("PlaceID");
            ModelState.Remove("VenueID");
            if (ModelState.IsValid)
            {
                string pID= await _idService.GetNextPlaceID();                
                string imagePath =await _fileService.SaveFileAsync(file,pID, "VenueImage");
                var place = new Place
                {
                    PlaceID = pID,
                    PlaceName = vm.PlaceName,
                    PlaceAddress = vm.PlaceAddress,
                    VenueImage = imagePath,
                    
                };

                if (place.Venue == null)
                {
                    place.Venue = new List<Venue>();
                }

                foreach (var item in vm.VenueItem)
                {
                    string vID = await _idService.GetNextVenueID(pID);
                    place.Venue.Add(new Venue
                    {
                        VenueID = vID,
                        PlaceID = pID,
                        VenueName = item.VenueName,
                        FloorName = item.FloorName,
                        AreaColor = item.AreaColor,
                        RowCount = item.RowCount,
                        SeatCount = item.SeatCount,
                        VenueStatusID = item.VenueStatusID
                    });

                }
                _context.Add(place);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateDropdownLists(vm.VenueItem.FirstOrDefault()?.VenueStatusID);
            return View(vm);
        }

        

        // GET: Places/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            

            var place = await _context.Place
                .Include(p => p.Venue) 
                .ThenInclude(v => v.VenueStatus)
                .FirstOrDefaultAsync(p => p.PlaceID == id);

            if (place == null) return NotFound();

            var vm = new VMPlaceCreate
            {
                PlaceID = place.PlaceID,
                PlaceName = place.PlaceName,
                PlaceAddress = place.PlaceAddress,
                VenueImages = place.VenueImage,
                VenueItem = place.Venue.Select(v => new VMVenueItem
                {
                    VenueID = v.VenueID,
                    VenueName = v.VenueName,
                    FloorName = v.FloorName,
                    AreaColor = v.AreaColor,
                    RowCount = v.RowCount,
                    SeatCount = v.SeatCount,
                    VenueStatusID = v.VenueStatusID,    
                    VenueStatusName = v.VenueStatus?.VenueStatusName
                }).ToList()
            };

            PopulateDropdownLists();
            return View(vm);
        }

        // POST: Places/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VMPlaceCreate vm, IFormFile? file)
        {
            // 移除 ID 驗證，因為我們會在後端處理
            ModelState.Remove("PlaceID");
            ModelState.Remove("VenueID");
            ModelState.Remove("VenueStatusID");
            ModelState.Remove("VenueStatusName");

            if (ModelState.IsValid)
            {
                // 1. 抓出資料庫現有的資料 (一定要 Include 區域)
                var dbPlace = await _context.Place
                    .Include(p => p.Venue)
                    .ThenInclude(v => v.VenueStatus)
                    .FirstOrDefaultAsync(p => p.PlaceID == vm.PlaceID);

                if (dbPlace == null) return NotFound();

                // 2. 更新場地基本資訊
                dbPlace.PlaceName = vm.PlaceName;
                dbPlace.PlaceAddress = vm.PlaceAddress;

                // 3. 處理圖片：如果有新檔案就換掉，沒有就沿用原本的路徑 (vm.VenueImages)
                if (file != null)
                {
                    dbPlace.VenueImage = await _fileService.SaveFileAsync(file, vm.PlaceID, "VenueImage");
                }
                else
                {
                    dbPlace.VenueImage = vm.VenueImages;
                }

                // --- 4. 區域 (Venue) 同步邏輯 ---

                // 取得前端傳回的所有 VenueID (排除掉新增加、還沒編號的空值)
                var vmVenueIds = vm.VenueItem.Where(v => !string.IsNullOrEmpty(v.VenueID)).Select(v => v.VenueID).ToList();

                // (A) 刪除：如果資料庫有，但前端沒傳回來的，代表被使用者刪掉了
                var toRemove = dbPlace.Venue.Where(v => !vmVenueIds.Contains(v.VenueID)).ToList();
                foreach (var r in toRemove) _context.Venue.Remove(r);


                // 先領一個「起點號碼」
                var baseVenueID = await _idService.GetNextVenueID(vm.PlaceID);
                //拆解它
                string prefix = baseVenueID.Substring(0, vm.PlaceID.Length); // 拿到 "C"
                int startSeq = int.Parse(baseVenueID.Substring(vm.PlaceID.Length)); // 拿到 2


                // (B) 更新或新增
                foreach (var item in vm.VenueItem)
                {
                    if (!string.IsNullOrEmpty(item.VenueID))
                    {
                        // 🔄 更新舊有的
                        var existing = dbPlace.Venue.FirstOrDefault(v => v.VenueID == item.VenueID);
                        if (existing != null)
                        {
                            existing.VenueName = item.VenueName;
                            existing.FloorName = item.FloorName;
                            existing.AreaColor = item.AreaColor;
                            existing.RowCount = item.RowCount;
                            existing.SeatCount = item.SeatCount;
                            existing.VenueStatusID = item.VenueStatusID;
                        }
                    }
                    else
                    {
                        // ✨ 新增剛加進來的
                        var finalID = $"{prefix}{startSeq:D2}";
                        dbPlace.Venue.Add(new Venue
                        {
                            VenueID = finalID,
                            PlaceID = dbPlace.PlaceID,
                            VenueName = item.VenueName,
                            FloorName = item.FloorName,
                            AreaColor = item.AreaColor,
                            RowCount = item.RowCount,
                            SeatCount = item.SeatCount,
                            VenueStatusID = item.VenueStatusID
                        });
                        startSeq++; // 新增完一筆，序號就 +1，在記憶體中自增，不回頭問 SQL，保證這批次不重複
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "更新失敗，可能有相關票券已售出，無法修改或刪除區域。");
                }
            }

            PopulateDropdownLists(vm.VenueItem.FirstOrDefault()?.VenueStatusID);
            return View(vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                // 1. 抓出場地，並包含區域 (Venue)
                var place = await _context.Place
                    .Include(p => p.Venue)
                    .FirstOrDefaultAsync(p => p.PlaceID == id);

                if (place == null) return Json(new { success = false, message = "找不到該地點。" });

                // 🚩 2. 先把圖片檔名備份起來 (因為 SaveChangesAsync 後物件狀態會改變)
                // 假設欄位名稱是 VenueImage
                string fileNameToDelete = place.VenueImage;

                // 3. 安全檢查：檢查是否已被票區使用
                bool isUsedInTicketsArea = await _context.TicketsArea
                    .AnyAsync(ta => ta.Venue.PlaceID == id);

                if (isUsedInTicketsArea)
                {
                    return Json(new { success = false, message = "此地點的區域已設定票價與場次，禁止刪除！" });
                }

                // 4. 執行資料庫刪除
                _context.Place.Remove(place);
                await _context.SaveChangesAsync();

                // 🚩 5. 資料庫刪除成功後，呼叫你的萬用檔案服務刪除實體圖檔
                // 參數順序依照你寫的：檔名, 第一層資料夾, 第二層資料夾
                if (!string.IsNullOrEmpty(fileNameToDelete))
                {
                    await _fileService.DeleteFileAsync(fileNameToDelete, "Photos", "VenueImage");
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "刪除失敗：" + ex.Message });
            }
        }


        private void PopulateDropdownLists(string? selectedStatus = null)
        {
            ViewData["VenueStatus"] = new SelectList(_context.VenueStatus, "VenueStatusID", "FullDisplayName", selectedStatus);
        }
    }
}
