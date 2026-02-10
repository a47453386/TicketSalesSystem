using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using TicketSalesSystem.DTOs;
using TicketSalesSystem.Helpers;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.ID;
using TicketSalesSystem.Service.Images;
using TicketSalesSystem.Service.IProgramme;
using TicketSalesSystem.ViewModel;
using TicketSalesSystem.ViewModel.CreateProgramme.CreateProgrammeStep;
using TicketSalesSystem.ViewModel.CreateProgramme.Item;
using TicketSalesSystem.ViewModel.EditProgramme;

namespace TicketSalesSystem.Controllers
{
    public class ProgrammeDTOController : Controller
    {
        private readonly IFileService _fileService;
        private readonly TicketsContext _context;
        private readonly IIDService _iIDService;
        private readonly IProgrammeService _programmeService;
        public ProgrammeDTOController(IFileService fileService, TicketsContext context, IIDService iIDService, IProgrammeService programmeService   )
        {

            _fileService = fileService;
            _context = context;
            _iIDService = iIDService;
            _programmeService = programmeService;
        }


        private ProgrammeDTO GetCurrentDTO()
        {
            return HttpContext.Session.GetObject<ProgrammeDTO>("programme") ?? new ProgrammeDTO();
        }
        private void SaveDTO(ProgrammeDTO dto)
        {
            HttpContext.Session.SetObject("programme", dto);
        }



        private void MapStep1ToDo(VMProgrammeStep1 vm, ProgrammeDTO dto)
        {
            dto.ProgrammeName = vm.ProgrammeName;
            dto.ProgrammeDescription = vm.ProgrammeDescription;
            dto.LimitPerOrder = vm.LimitPerOrder;
            dto.OnShelfTime = vm.OnShelfTime;
            dto.PlaceID = vm.PlaceID;
            dto.ProgrammeStatusID = "D"; // 預設草稿
        }
        private void MapStep1ToVM(ProgrammeDTO dto, VMProgrammeStep1 vm)
        {
            vm.ProgrammeName = dto.ProgrammeName;
            vm.ProgrammeDescription = dto.ProgrammeDescription;
            vm.LimitPerOrder = dto.LimitPerOrder;
            vm.OnShelfTime = dto.OnShelfTime;
            vm.PlaceID = dto.PlaceID;
            vm.ProgrammeStatusID = dto.ProgrammeStatusID;
        }



        [HttpGet]
        public IActionResult CreateStep1()
        {
            //從 Session 取出 DTO，如果沒有就建立一個新的
            var dto = GetCurrentDTO();

            //建立一個空白的 VM
            var vm = new VMProgrammeStep1();

            //將 DTO 的資料映射到 VM
            MapStep1ToVM(dto, vm);

            ViewData["PlaceID"] = new SelectList(_context.Place.ToList(), "PlaceID", "PlaceName", vm.PlaceID);
            ViewData["ProgrammeStatusID"] = new SelectList(_context.ProgrammeStatus.ToList(), "ProgrammeStatusID", "ProgrammeStatusName", dto.ProgrammeStatusID);
            //將 VM 傳遞給 View
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStep1(VMProgrammeStep1 vm)
        {
            //驗證輸入
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            //從 Session 取出 DTO，如果沒有就建立一個新的
            var dto = GetCurrentDTO();
            //將 VM 的資料映射到 DTO
            MapStep1ToDo(vm, dto);
            //將 DTO 存回 Session
            SaveDTO(dto);
            //導向下一步
            return RedirectToAction("CreateStep2");
        }



        private void MapStep2ToDo(VMProgrammeStep2 vm, ProgrammeDTO dto)
        {

            dto.Session = vm.Session?
                .Select(s => new SessionDTO
                {
                    SaleStartTime = s.SaleStartTime,
                    SaleEndTime = s.SaleEndTime,
                    StartTime = s.StartTime
                })
                .ToList() ?? new List<SessionDTO>();

        }

        private void MapStep2ToVM(ProgrammeDTO dto, VMProgrammeStep2 vm)
        {
            vm.Session = dto.Session?
                .Select(s => new VMSessionItem
                {
                    SaleStartTime = s.SaleStartTime,
                    SaleEndTime = s.SaleEndTime,
                    StartTime = s.StartTime
                })
                .ToList() ?? new List<VMSessionItem>();
        }


        [HttpGet]
        public IActionResult CreateStep2()
        {

            //取得現有的 DTO (使用你之前抽出來的 GetCurrentDto)
            var dto = GetCurrentDTO();

            //建立 Step 2 的 VM
            var vm = new VMProgrammeStep2();

            vm.PlaceID = dto.PlaceID;

            if (dto.Session != null && dto.Session.Any())
            {
                MapStep2ToVM(dto, vm);
            }
            else
            {
                // 如果是第一次進來，預設給一筆空白場次，方便前端產生第一個輸入框
                var vmF = new VMSessionItem
                {
                    StartTime = DateTime.Now,
                    SaleStartTime = DateTime.Now,
                    SaleEndTime = DateTime.Now
                };
                vm.Session.Add(vmF);
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStep2(VMProgrammeStep2 vm)
        {
            // 🚩 如果 ModelState 報錯說 PlaceID 是必填，通常是因為 View 沒傳回來
            // 我們可以從 Session 補救它
            if (string.IsNullOrEmpty(vm.PlaceID))
            {
                var dtoInSession = GetCurrentDTO();
                vm.PlaceID = dtoInSession.PlaceID;

                // 補救後要告訴 ModelState 這個欄位現在過關了
                ModelState.Remove("PlaceID");
            }

            //驗證輸入
            if (!ModelState.IsValid)
            {
                return View(vm);

            }
            var dto = GetCurrentDTO();

            //將 VM 的資料映射到 DTO
            if (vm.Session == null || !vm.Session.Any())
            {
                ModelState.AddModelError("", "請至少新增一個場次");
                return View(vm);
            }
            MapStep2ToDo(vm, dto);
            //將 DTO 存回 Session
            SaveDTO(dto);
            //導向下一步
            return RedirectToAction("CreateStep3");
        }


        private void MapStep3ToDo(VMProgrammeStep3 vm, ProgrammeDTO dto)
        {
            dto.VenueID = vm.VenueID;
            dto.TicketsAreaStatusID = vm.TicketsAreaStatusID;

            dto.Session ??= new List<SessionDTO>();
            var allTicketsAreas = new List<TicketsAreaDTO>();

            // 🚩 這裡要巡覽的是 vm.Session (前端傳回來的場次)
            foreach (var sessionVM in vm.Session)
            {
                var targetSession = dto.Session.FirstOrDefault(s => s.SessionID == sessionVM.SessionID);
                if (targetSession != null)
                {
                    // 更新該場次的票區清單
                    targetSession.TicketsArea = sessionVM.TicketsArea.Select(ta => new TicketsAreaDTO
                    {
                        TicketsAreaID = ta.TicketsAreaID,
                        TicketsAreaName = ta.TicketsAreaName,
                        Price = ta.Price,
                        RowCount = ta.RowCount,
                        SeatCount = ta.SeatCount,
                        VenueID = vm.VenueID,
                        TicketsAreaStatusID = ta.TicketsAreaStatusID ?? "A"
                    }).ToList();

                    // 收集所有票區到彙整清單中
                    foreach (var ta in targetSession.TicketsArea)
                    {
                        allTicketsAreas.Add(ta);
                    }
                }
            }

            // 🚩 這行要放在 foreach 迴圈外面，否則會重複執行多次
            dto.TicketsArea = allTicketsAreas.GroupBy(x => x.TicketsAreaName)
                                             .Select(g => g.First())
                                             .ToList();
        }
        private void MapStep3ToVM(ProgrammeDTO dto, VMProgrammeStep3 vm)
        {
            vm.VenueID = dto.VenueID;
            vm.TicketsAreaStatusID = dto.TicketsAreaStatusID;

            if (dto.Session != null)
            {
                vm.Session = dto.Session.Select(s => new VMSessionItem
                {
                    SessionID = s.SessionID,
                    StartTime = s.StartTime,
                    // 將 DTO 裡的票區轉回 VM 格式
                    TicketsArea = s.TicketsArea?.Select(ta => new VMTicketsAreaItem
                    {
                        TicketsAreaID = ta.TicketsAreaID,
                        TicketsAreaName = ta.TicketsAreaName,
                        RowCount = ta.RowCount,
                        SeatCount = ta.SeatCount,
                        Price = ta.Price,
                        VenueID = ta.VenueID,
                        TicketsAreaStatusID = ta.TicketsAreaStatusID
                    }).ToList() ?? new List<VMTicketsAreaItem>()
                }).ToList();
            }
        }
        [HttpGet]
        public IActionResult CreateStep3()
        {
            //取得現有的 DTO (使用你之前抽出來的 GetCurrentDto)
            var dto = GetCurrentDTO();

            if (dto == null) return RedirectToAction("CreateStep1");
            //建立 Step 3 的 VM
            var vm = new VMProgrammeStep3();


            ViewData["VenueID"] = new SelectList(_context.Venue.ToList(), "VenueID", "VenueName", dto.VenueID);

            MapStep3ToVM(dto, vm);

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStep3(VMProgrammeStep3 vm)
        {

            //驗證輸入
            if (!ModelState.IsValid)
            {
                ViewData["VenueID"] = new SelectList(_context.Venue.ToList(), "VenueID", "VenueName", vm.VenueID);

                return View(vm);
            }
            //從 Session 取出 DTO，如果沒有就建立一個新的
            var dto = GetCurrentDTO();
            //將 VM 的資料映射到 DTO
            if (vm.Session.Any(s => s.TicketsArea == null || !s.TicketsArea.Any()))
            {
                ModelState.AddModelError("", "每個區域/場次請至少新增一個票區");
                return View(vm);
            }
            MapStep3ToDo(vm, dto);
            //將 DTO 存回 Session
            SaveDTO(dto);
            //導向下一步
            return RedirectToAction("CreateStep4");
        }


        [HttpGet]
        public IActionResult CreateStep4()
        {
            //取得現有的 DTO (使用你之前抽出來的 GetCurrentDto)
            var dto = GetCurrentDTO();
            if (dto == null) return RedirectToAction("CreateStep1");

            //建立 Step 4 的 VM
            var vm = new VMProgrammeStep4
            {
                CoverImage = dto.CoverImage,
                SeatImage = dto.SeatImage,
                DescriptionImages = dto.DescriptionImage?.Select(di => new VMDescriptionImageItem
                {
                    DescriptionImageID = di.DescriptionImageID,
                    DescriptionImageName = di.DescriptionImageName,
                    TempUrl = di.TempUrl
                }).ToList() ?? new List<VMDescriptionImageItem>()
            };
            return View(vm);


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStep4(VMProgrammeStep4 vm)
        {
            var dto = GetCurrentDTO();
            if (dto == null) return RedirectToAction("CreateStep1");


            //dto.DescriptionImages ??= new List<DescriptionImageDTO>();
            string idSource = $"{DateTime.Now:HHmmss}{new Random().Next(10, 99)}";
            // 1. 處理封面圖 (Cover Image)
            if (vm.CoverImageFile != null)
            {
                // 建議封面圖可以用活動名稱或固定前綴，這裡先用 Guid 確保唯一
                string coverID = $"C{idSource}";
                var savedCoverPath = await _fileService.SaveFileAsync(vm.CoverImageFile, coverID, "CoverImage");
                dto.CoverImage = savedCoverPath; // 存入 DTO
            }

            // 2. 處理座位圖 (Seat Image)
            if (vm.SeatImageFile != null)
            {
                string seatID = $"S{idSource}";
                var savedSeatPath = await _fileService.SaveFileAsync(vm.SeatImageFile, seatID, "SeatImage");
                dto.SeatImage = savedSeatPath; // 存入 DTO
            }

            if (vm.DescriptionImageFiles == null)
            {
                return View(vm);
            }

            foreach (var file in vm.DescriptionImageFiles)
            {
                //先產生ID
                string newID = Guid.NewGuid().ToString();

                //呼叫多載A
                var savedPath = await _fileService.SaveFileAsync(file, newID, "DescriptionImage");
                dto.DescriptionImage.Add(new DescriptionImageDTO
                {
                    DescriptionImageID = newID,
                    DescriptionImageName = file.FileName,
                    TempUrl = savedPath
                });
            }

            SaveDTO(dto);
            return RedirectToAction("CreateConfirm");
        }

        [HttpGet]
        public IActionResult CreateConfirm()
        {
            try
            {
                var dto = GetCurrentDTO();
                if (dto == null)
                {
                    return RedirectToAction("CreateStep1");
                }

                dto.Session ??= new List<SessionDTO>();
                dto.TicketsArea ??= new List<TicketsAreaDTO>();
                dto.DescriptionImage ??= new List<DescriptionImageDTO>();

                if (!string.IsNullOrEmpty(dto.PlaceID))
                    ViewData["PlaceName"] = _context.Place.Find(dto.PlaceID)?.PlaceName;

                if (!string.IsNullOrEmpty(dto.VenueID))
                    ViewData["VenueName"] = _context.Venue.Find(dto.VenueID)?.VenueName;
                return View(dto);
            }
            catch (Exception ex)
            {
                return Content($"真正的錯誤訊息：{ex.Message} \n 堆疊追蹤：{ex.StackTrace}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConfirm(IFormCollection col)
        {
            var dto = GetCurrentDTO();

            if (dto == null || string.IsNullOrEmpty(dto.ProgrammeName))
            {
                return RedirectToAction("CreateStep1");
            }

            using (var transction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    string pid = await _iIDService.GetNextProgrammeID();
                    var programme = new Programme
                    {
                        ProgrammeID = pid,
                        ProgrammeName = dto.ProgrammeName,
                        ProgrammeDescription = dto.ProgrammeDescription,
                        CreatedTime = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        CoverImage = dto.CoverImage,
                        SeatImage = dto.SeatImage,
                        LimitPerOrder = dto.LimitPerOrder,
                        OnShelfTime = dto.OnShelfTime,
                        ProgrammeStatusID = dto.ProgrammeStatusID ?? "D",
                        EmployeeID = "A23025",
                        PlaceID = dto.PlaceID
                    };
                    _context.Programme.Add(programme);
                    await _context.SaveChangesAsync();

                    foreach (var sDto in dto.Session)
                    {
                        string sid = await _iIDService.GetNextSessionID(pid);
                        var session = new Session
                        {
                            SessionID = sid,
                            ProgrammeID = pid,
                            StartTime = sDto.StartTime,
                            SaleStartTime = sDto.SaleStartTime,
                            SaleEndTime = sDto.SaleEndTime
                        };
                        _context.Session.Add(session);
                        await _context.SaveChangesAsync();


                        foreach (var aDto in dto.TicketsArea)
                        {
                            string taid = await _iIDService.GetNextTicketsAreaID(sid);
                            var ticketsArea = new TicketsArea
                            {
                                TicketsAreaID = taid,
                                SessionID = sid,
                                VenueID = aDto.VenueID,
                                TicketsAreaName = aDto.TicketsAreaName,
                                RowCount = aDto.RowCount,
                                SeatCount = aDto.SeatCount,
                                Price = aDto.Price,
                                TicketsAreaStatusID = aDto.TicketsAreaStatusID
                            };
                            _context.TicketsArea.Add(ticketsArea);
                            await _context.SaveChangesAsync();
                        }
                    }

                    if (dto.DescriptionImage != null)
                    {
                        foreach (var diDto in dto.DescriptionImage)
                        {

                            var descriptionImage = new DescriptionImage
                            {
                                DescriptionImageID = diDto.DescriptionImageID,
                                ProgrammeID = pid,
                                DescriptionImageName = diDto.DescriptionImageName,
                                ImagePath = diDto.TempUrl
                            };
                            _context.DescriptionImage.Add(descriptionImage);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transction.CommitAsync();

                    // 修正：清除正確的 Session Key
                    HttpContext.Session.Remove("programme");

                    return RedirectToAction("Index", "Programmes", new { msg = "活動建立成功！" });

                }
                catch (Exception ex)
                {
                    await transction.RollbackAsync();
                    var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "無底層訊息";

                    return Content($"存檔失敗！<br/>" +
                                   $"主錯誤：{ex.Message}<br/>" +
                                   $"底層原因：{innerMessage}"); // 這裡會直接寫「String or binary data would be truncated」或是「FK conflict」
                }
            }

        }






        [HttpGet]
        public IActionResult GetVenues(string placeId) // 🚩 確保 placeId 名稱與 AJAX 傳送的一致
        {
            var venues = _context.Venue.Where(v => v.PlaceID == placeId).ToList();
            return Json(venues);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if(id==null) return NotFound();

            // 1. 關鍵：使用 Include 與 ThenInclude 抓取三層資料
            var programme = await _context.Programme
                .Include(p => p.DescriptionImage) // 描述圖片層
                .Include(p => p.Session)          // 場次層
                    .ThenInclude(s => s.TicketsArea) // 票區層 (場次底下的票區)
                .FirstOrDefaultAsync(m => m.ProgrammeID == id);

            if (programme == null) return NotFound();

            // 2. 將 Entity 轉換為 ViewModel (DTO)
            var vm = new VMProgrammeEdit
            {
                ProgrammeID = programme.ProgrammeID,
                ProgrammeName = programme.ProgrammeName,
                ProgrammeDescription = programme.ProgrammeDescription,
                CoverImage = programme.CoverImage,
                SeatImage = programme.SeatImage,
                LimitPerOrder = programme.LimitPerOrder,
                OnShelfTime = programme.OnShelfTime,
                PlaceID = programme.PlaceID,
                ProgrammeStatusID = programme.ProgrammeStatusID,
                VenueID= programme.Session.FirstOrDefault()?.TicketsArea.FirstOrDefault()?.VenueID, // 從第一個場次的第一個票區抓 VenueID


                // 轉換描述圖片清單 (讓前端顯示縮圖)
                DescriptionImage = programme.DescriptionImage.Select(di => new DescriptionImageDTO
                {
                    DescriptionImageID = di.DescriptionImageID,
                    DescriptionImageName = di.DescriptionImageName,
                    TempUrl = di.ImagePath
                }).ToList(),

                // 轉換場次與票區
                Session = programme.Session.Select(s => new VMSessionItem
                {
                    SessionID = s.SessionID,
                    StartTime = s.StartTime,
                    SaleStartTime = s.SaleStartTime,
                    SaleEndTime = s.SaleEndTime,
                    TicketsArea = s.TicketsArea.Select(ta => new VMTicketsAreaItem
                    {
                        TicketsAreaID = ta.TicketsAreaID,
                        TicketsAreaName = ta.TicketsAreaName,
                        RowCount = ta.RowCount,
                        SeatCount = ta.SeatCount,
                        Price = ta.Price,
                        VenueID = ta.VenueID,
                        TicketsAreaStatusID = ta.TicketsAreaStatusID
                    }).ToList()
                }).ToList()
            };
            ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceID", "PlaceName", programme.PlaceID);
            ViewData["ProgrammeStatusID"] = new SelectList(_context.ProgrammeStatus, "ProgrammeStatusID", "ProgrammeStatusName", programme.ProgrammeStatusID);
            
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, VMProgrammeEdit vm)
        {
            if (id != vm.ProgrammeID) return NotFound();
             
            if (ModelState.IsValid)
            {
                // 取得包含所有子層的數據
                var dbProgramme = await _context.Programme
                    .Include(p => p.Session).ThenInclude(s => s.TicketsArea).Include(p => p.DescriptionImage)
                    .FirstOrDefaultAsync(p => p.ProgrammeID == id);

                if (dbProgramme == null) return NotFound();

                try
                {
                    // 呼叫 Service 執行複雜同步
                    await _programmeService.SyncProgrammeDetailsAsync(dbProgramme, vm);
                    await _programmeService.SyncImagesAsync(dbProgramme, vm);
                    int rowsAffected = await _context.SaveChangesAsync();
                    Console.WriteLine($"更新了 {rowsAffected} 筆資料");

                    TempData["SuccessMessage"] = "活動資料已成功更新！";
                    return RedirectToAction("Index", "Programmes");
                }
                catch (DbUpdateException ex)
                {
                    var innerError = ex.InnerException?.Message ?? ex.Message;
                    ModelState.AddModelError("", "資料庫存檔失敗！原因：可能是有訂單關聯而無法刪除場次。原始訊息：" + innerError);

                    // 偵錯用
                    Console.WriteLine("SQL Error: " + innerError);

                    // 返回 View，vm 帶回前端，使用者才不會看到空白或崩潰
                    return View(vm); ;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "存檔失敗: " + ex.Message);

                    var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "無底層訊息";

                    return Content($"存檔失敗！<br/>" +
                                   $"主錯誤：{ex.Message}<br/>" +
                                   $"底層原因：{innerMessage}");

                }
            }
            // 若失敗，重新載入必要的下拉選單資料 (如 PlaceID) 並回傳 View
            ViewBag.PlaceID = new SelectList(_context.Place, "PlaceID", "PlaceName", vm.PlaceID);
            return RedirectToAction("Index", "Programmes");
        }















    }
}
