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
        public ProgrammeDTOController(IFileService fileService, TicketsContext context, IIDService iIDService)
        {

            _fileService = fileService;
            _context = context;
            _iIDService = iIDService;
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

            dto.Sessions = vm.Sessions?
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
            vm.Sessions = dto.Sessions?
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

            if (dto.Sessions != null && dto.Sessions.Any())
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
                vm.Sessions.Add(vmF);
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
            if (vm.Sessions == null || !vm.Sessions.Any())
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

            // 假設 DTO 層級也有對應的 Sessions
            dto.Sessions ??= new List<SessionDTO>();


            foreach (var sessionVM in vm.Sessions)
            {
                var targetSession = dto.Sessions.FirstOrDefault(s => s.SessionID == sessionVM.SessionID);
                if (targetSession != null)
                {
                    // 將該場次下的票區清單存入 DTO
                    targetSession.TicketsArea = sessionVM.TicketsArea.Select(ta => new VMTicketsAreaItem
                    {
                        TicketsAreaID = ta.TicketsAreaID,
                        TicketsAreaName = ta.TicketsAreaName,
                        Price = ta.Price,
                        RowCount = ta.RowCount,
                        SeatCount = ta.SeatCount,
                        VenueID = vm.VenueID, // 統一使用 Step3 選擇的場地
                        TicketsAreaStatusID = ta.TicketsAreaStatusID ?? "A"
                    }).ToList();
                }
            }
        }
        private void MapStep3ToVM(ProgrammeDTO dto, VMProgrammeStep3 vm)
        {
            vm.VenueID = dto.VenueID;
            vm.TicketsAreaStatusID = dto.TicketsAreaStatusID;

            // 從 DTO 的 Sessions 抓取資料倒填回 VM
            vm.Sessions = dto.Sessions.Select(s => new VMSessionItem
            {
                SessionID = s.SessionID,
                TicketsArea = s.TicketsArea?.Select(ta => new VMTicketsAreaItem
                {
                    TicketsAreaID = ta.TicketsAreaID,
                    TicketsAreaName = ta.TicketsAreaName,
                    RowCount = ta.RowCount,
                    SeatCount = ta.SeatCount,
                    Price = ta.Price,
                    VenueID = ta.VenueID,
                    TicketsAreaStatusID = ta.TicketsAreaStatusID
                }).ToList() ?? new List<VMTicketsAreaItem> { new VMTicketsAreaItem() } // 若無票區則預設給一筆空白
            }).ToList();
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
            if (vm.Sessions.Any(s => s.TicketsArea == null || !s.TicketsArea.Any()))
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
                DescriptionImages = dto.DescriptionImages?.Select(di => new VMDescriptionImageItem
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
                dto.DescriptionImages.Add(new DescriptionImageDTO
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

                dto.Sessions ??= new List<SessionDTO>();
                dto.TicketsAreas ??= new List<TicketsAreaDTO>();
                dto.DescriptionImages ??= new List<DescriptionImageDTO>();

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

                    foreach (var sDto in dto.Sessions)
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


                        foreach (var aDto in dto.TicketsAreas)
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

                    if (dto.DescriptionImages != null)
                    {
                        foreach (var diDto in dto.DescriptionImages)
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

                    return RedirectToAction("Index", "Home", new { msg = "活動建立成功！" });

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
                DescriptionImages = programme.DescriptionImage.Select(di => new DescriptionImageDTO
                {
                    DescriptionImageID = di.DescriptionImageID,
                    DescriptionImageName = di.DescriptionImageName,
                    TempUrl = di.ImagePath
                }).ToList(),

                // 轉換場次與票區
                Sessions = programme.Session.Select(s => new SessionDTO
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

            return View(vm);
        }








        //// 在 Controller 加入這個方法
        //[HttpGet]
        //public IActionResult GetVenues(string placeId)
        //{
        //    var venues = _context.Venue
        //        .Where(v => v.PlaceID == placeId)
        //        .Select(v => new { v.VenueID, v.VenueName })
        //        .ToList();
        //    return Json(venues);
        //}


        //[HttpGet]
        //public async Task<IActionResult> Edit(string id)
        //{
        //    var programme = await _context.Programme
        //                    .Include(p => p.DescriptionImage)
        //                    .Include(p => p.Session)
        //                    .ThenInclude(s => s.TicketsArea)
        //                    .AsNoTracking() // 關鍵點：不要追蹤這份舊資料
        //                    .FirstOrDefaultAsync(p => p.ProgrammeID == id);

        //    if (programme == null) return NotFound();

        //    ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceID", "PlaceName", programme.PlaceID);
        //    ViewData["ProgrammeStatusID"] = new SelectList(_context.ProgrammeStatus, "ProgrammeStatusID", "ProgrammeStatusName", programme.ProgrammeStatusID);

        //    var vm = new VMProgrammeEdit
        //    {
        //        ProgrammeID = programme.ProgrammeID,
        //        ProgrammeName = programme.ProgrammeName,
        //        ProgrammeDescription = programme.ProgrammeDescription,
        //        CoverImage = programme.CoverImage,
        //        SeatImage = programme.SeatImage,
        //        LimitPerOrder = programme.LimitPerOrder,
        //        OnShelfTime = programme.OnShelfTime,
        //        PlaceID = programme.PlaceID,
        //        ProgrammeStatusID = programme.ProgrammeStatusID,
        //        Sessions = programme.Session
        //            .Select(s => new SessionDTO
        //            {
        //                SaleStartTime = s.SaleStartTime,
        //                SaleEndTime = s.SaleEndTime,
        //                StartTime = s.StartTime,
        //                TicketsArea = s.TicketsArea.Select(a => new VMTicketsAreaItem
        //                {
        //                    TicketsAreaID = a.TicketsAreaID,
        //                    VenueID = a.VenueID,
        //                    TicketsAreaName = a.TicketsAreaName,
        //                    RowCount = a.RowCount,
        //                    SeatCount = a.SeatCount,
        //                    Price = a.Price,
        //                    TicketsAreaStatusID = a.TicketsAreaStatusID
        //                }).ToList()
        //            }
        //           )
        //            .ToList(),
        //    };

        //    return View(vm);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(string id, VMProgrammeEdit vm)
        //{
        //    // 1. 移除不必要的驗證錯誤
        //    ModelState.Remove("VenueID");
        //    ModelState.Remove("TicketsAreaStatusID");
        //    var sessionKeys = ModelState.Keys.Where(k => k.Contains("SessionID") || k.Contains("TicketsAreaID") || k.Contains("TicketsAreaStatusID"));
        //    foreach (var key in sessionKeys) ModelState.Remove(key);

        //    if (!ModelState.IsValid)
        //    {
        //        ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceID", "PlaceName", vm.PlaceID);
        //        return View(vm);
        //    }

        //    // 2. 抓取現有資料 (含 Session, TicketsArea, DescriptionImage)
        //    var programme = await _context.Programme
        //        .Include(p => p.Session).ThenInclude(s => s.TicketsArea)
        //        .Include(p => p.DescriptionImage)
        //        .FirstOrDefaultAsync(p => p.ProgrammeID == id);

        //    if (programme == null) return NotFound();

        //    // --- A. 基本資料與圖片處理 ---
        //    programme.ProgrammeName = vm.ProgrammeName;
        //    programme.ProgrammeDescription = vm.ProgrammeDescription;
        //    programme.LimitPerOrder = vm.LimitPerOrder;
        //    programme.OnShelfTime = vm.OnShelfTime;
        //    programme.PlaceID = vm.PlaceID;
        //    programme.ProgrammeStatusID = vm.ProgrammeStatusID;

        //    string uniqueSuffix = $"{DateTime.Now:HHmmss}{new Random().Next(10, 99)}";
        //    if (vm.CoverImageFile != null)
        //        programme.CoverImage = await _fileService.SaveFileAsync(vm.CoverImageFile, $"C{uniqueSuffix}", "CoverImage");
        //    if (vm.SeatImageFile != null)
        //        programme.SeatImage = await _fileService.SaveFileAsync(vm.SeatImageFile, $"S{uniqueSuffix}", "SeatImage");

        //    // 處理描述圖片刪除
        //    if (vm.DeleteImageIds != null && vm.DeleteImageIds.Any())
        //    {
        //        var imgsToDelete = programme.DescriptionImage.Where(di => vm.DeleteImageIds.Contains(di.DescriptionImageID)).ToList();
        //        _context.DescriptionImage.RemoveRange(imgsToDelete);
        //    }
        //    // 處理描述圖片新增
        //    if (vm.DescriptionImageFiles != null)
        //    {
        //        foreach (var file in vm.DescriptionImageFiles)
        //        {
        //            string newID = Guid.NewGuid().ToString();
        //            string path = await _fileService.SaveFileAsync(file, newID, "DescriptionImage");
        //            programme.DescriptionImage.Add(new DescriptionImage
        //            {
        //                DescriptionImageID = newID,
        //                DescriptionImageName = file.FileName,
        //                ImagePath = path
        //            });
        //        }
        //    }

        //    // --- B. 處理「場次刪除」 ---
        //    var vmSessionIds = vm.Sessions.Select(s => s.SessionID).ToList();
        //    var sessionsToRemove = programme.Session.Where(s => !vmSessionIds.Contains(s.SessionID)).ToList();
        //    foreach (var s in sessionsToRemove)
        //    {
        //        // 先手動刪除票區
        //        _context.TicketsArea.RemoveRange(s.TicketsArea);
        //        _context.Session.Remove(s);
        //    }

        //    // --- C. 處理「場次更新與新增」 ---
        //    foreach (var sessionVM in vm.Sessions)
        //    {
        //        var dbSession = programme.Session.FirstOrDefault(s => s.SessionID == sessionVM.SessionID);

        //        if (dbSession != null) // 更新現有場次
        //        {
        //            _context.Entry(dbSession).CurrentValues.SetValues(sessionVM);

        //            // 同步票區
        //            var vmAreaIds = sessionVM.TicketsArea.Select(a => a.TicketsAreaID).ToList();                   
                                        
        //            // 1. 刪除票區
        //            var areasToRemove = dbSession.TicketsArea.Where(a => !vmAreaIds.Contains(a.TicketsAreaID)).ToList();
        //            _context.TicketsArea.RemoveRange(areasToRemove);

        //            // 2. 更新或新增票區
        //            foreach (var areaVM in sessionVM.TicketsArea)
        //            {
        //                var dbArea = dbSession.TicketsArea.FirstOrDefault(a => a.TicketsAreaID == areaVM.TicketsAreaID);
        //                if (dbArea != null)
        //                {
        //                    _context.Entry(dbArea).CurrentValues.SetValues(areaVM);
        //                    dbArea.VenueID = areaVM.VenueID ?? vm.VenueID;
        //                }
        //                else
        //                {
        //                    string taid = await _iIDService.GetNextTicketsAreaID(dbSession.SessionID);
        //                    dbSession.TicketsArea.Add(new TicketsArea
        //                    {
        //                        TicketsAreaID = taid,
        //                        TicketsAreaName = areaVM.TicketsAreaName,
        //                        Price = areaVM.Price,
        //                        RowCount = areaVM.RowCount,
        //                        SeatCount = areaVM.SeatCount,
        //                        VenueID = areaVM.VenueID ?? vm.VenueID,
        //                        TicketsAreaStatusID = "A"
        //                    });
        //                }
        //            }
        //        }
        //        else // 新增場次
        //        {
        //            string sid = await _iIDService.GetNextSessionID(programme.ProgrammeID);
        //            var sessionToAdd = new Session
        //            {
        //                SessionID = sid,
        //                ProgrammeID = programme.ProgrammeID,
        //                StartTime = sessionVM.StartTime,
        //                SaleStartTime = sessionVM.SaleStartTime,
        //                SaleEndTime = sessionVM.SaleEndTime,
        //                TicketsArea = new List<TicketsArea>()
        //            };
        //            foreach (var areaVM in sessionVM.TicketsArea)
        //            {
        //                string taid = await _iIDService.GetNextTicketsAreaID(sid);
        //                sessionToAdd.TicketsArea.Add(new TicketsArea
        //                {
        //                    TicketsAreaID = taid,
        //                    TicketsAreaName = areaVM.TicketsAreaName,
        //                    Price = areaVM.Price,
        //                    RowCount = areaVM.RowCount,
        //                    SeatCount = areaVM.SeatCount,
        //                    VenueID = areaVM.VenueID ?? vm.VenueID,
        //                    TicketsAreaStatusID = "A"
        //                });
        //            }
        //            programme.Session.Add(sessionToAdd);
        //        }
        //    }

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", "存檔失敗: " + ex.Message);
        //        ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceID", "PlaceName", vm.PlaceID);
                
        //        var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "無底層訊息";

        //        return Content($"存檔失敗！<br/>" +
        //                       $"主錯誤：{ex.Message}<br/>" +
        //                       $"底層原因：{innerMessage}");
        //    }
        //}



        //private async Task SyncTicketsAreas(Session dbSession, SessionDTO vmSession, string? defaultVenueID)
        //{
        //    var vmAreaIds = vmSession.TicketsArea
        //        .Where(a => !string.IsNullOrEmpty(a.TicketsAreaID))
        //        .Select(a => a.TicketsAreaID).ToList();

        //    // A. 刪除：找出資料庫有，但前端傳回清單中沒有的
        //    var areasToRemove = dbSession.TicketsArea
        //        .Where(a => !vmAreaIds.Contains(a.TicketsAreaID))
        //        .ToList();

        //    if (areasToRemove.Any())
        //    {
        //        _context.TicketsArea.RemoveRange(areasToRemove);
        //    }

        //    // B. 更新或新增
        //    foreach (var areaVM in vmSession.TicketsArea)
        //    {
        //        if (!string.IsNullOrEmpty(areaVM.TicketsAreaID))
        //        {
        //            // --- 更新現有票區 ---
        //            // 直接從 dbSession 的追蹤集合中找，不要去資料庫重新撈
        //            var dbArea = dbSession.TicketsArea.FirstOrDefault(a => a.TicketsAreaID == areaVM.TicketsAreaID);
        //            if (dbArea != null)
        //            {
        //                // 使用 SetValues 進行屬性對應，這不會觸發 Tracking 衝突
        //                _context.Entry(dbArea).CurrentValues.SetValues(areaVM);
        //                dbArea.VenueID = areaVM.VenueID ?? defaultVenueID;
        //            }
        //        }
        //        else
        //        {
        //            // --- 新增票區 ---
        //            string taid = await _iIDService.GetNextTicketsAreaID(dbSession.SessionID);
        //            dbSession.TicketsArea.Add(new TicketsArea
        //            {
        //                TicketsAreaID = taid,
        //                TicketsAreaName = areaVM.TicketsAreaName,
        //                Price = areaVM.Price,
        //                RowCount = areaVM.RowCount,
        //                SeatCount = areaVM.SeatCount,
        //                VenueID = areaVM.VenueID ?? defaultVenueID,
        //                TicketsAreaStatusID = "A"
        //            });
        //        }
        //    }
        //}













        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(string id, VMProgrammeEdit vm)
        //{
        //    if (id != vm.ProgrammeID) return NotFound();

        //    // 1. 移除那些在 VM 頂層但不該在那裡的驗證 (因為它們在內層 DTO 裡)
        //    ModelState.Remove("VenueID");
        //    ModelState.Remove("TicketsAreaStatusID");

        //    // 2. 移除所有 Sessions 內層 ID 的強制驗證 (因為新增的場次還沒 ID)
        //    // 這是最暴力的解決方式，但也最有效
        //    var sessionKeys = ModelState.Keys.Where(k => k.Contains("SessionID") || k.Contains("TicketsAreaID") || k.Contains("TicketsAreaStatusID"));
        //    foreach (var key in sessionKeys)
        //    {
        //        ModelState.Remove(key);
        //    }




        //    if (!ModelState.IsValid)
        //    {
        //        // 重新填入下拉選單並回傳 View
        //        ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceID", "PlaceName", vm.PlaceID);
        //        return View(vm);
        //    }

        //    // 1. 抓取資料庫現有資料（包含關聯的 Session 和 TicketsArea）
        //    var programme = await _context.Programme
        //        .Include(p => p.Session)
        //        .ThenInclude(s => s.TicketsArea)
        //        .FirstOrDefaultAsync(p => p.ProgrammeID == id);

        //    if (programme == null) return NotFound();

        //    ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceID", "PlaceName", programme.PlaceID);
        //    ViewData["ProgrammeStatusID"] = new SelectList(_context.ProgrammeStatus, "ProgrammeStatusID", "ProgrammeStatusName", programme.ProgrammeStatusID);

        //    // 2. 更新活動主檔欄位
        //    programme.ProgrammeName = vm.ProgrammeName;
        //    programme.ProgrammeDescription = vm.ProgrammeDescription;
        //    programme.LimitPerOrder = vm.LimitPerOrder;
        //    programme.OnShelfTime = vm.OnShelfTime;
        //    programme.PlaceID = vm.PlaceID;
        //    programme.ProgrammeStatusID = vm.ProgrammeStatusID;

        //    // 3. 處理圖片更新 (如果有上傳新圖才更新)
        //    string uniqueSuffix = $"{DateTime.Now:HHmmss}{new Random().Next(10, 99)}";
        //    if (vm.CoverImageFile != null)
        //    {
        //        programme.CoverImage = await _fileService.SaveFileAsync(vm.CoverImageFile, $"C{uniqueSuffix}", "CoverImage");
        //    }

        //    if (vm.SeatImageFile != null) // 如果使用者有上傳新座位圖
        //    {
        //        programme.SeatImage = await _fileService.SaveFileAsync(vm.SeatImageFile, $"S{uniqueSuffix}", "SeatImage");
        //    }

        //    // 1. 處理刪除舊圖片
        //    if (vm.DeleteImageIds != null && vm.DeleteImageIds.Any())
        //    {
        //        var imagesToDelete = await _context.DescriptionImage
        //            .Where(di => vm.DeleteImageIds.Contains(di.DescriptionImageID))
        //            .ToListAsync();

        //        // 這裡建議也要呼叫 _fileService 刪除實體檔案，避免浪費空間
        //        _context.DescriptionImage.RemoveRange(imagesToDelete);
        //    }

        //    // 2. 處理新增圖片
        //    if (vm.DescriptionImageFiles != null && vm.DescriptionImageFiles.Any())
        //    {
        //        int count = 1;
        //        foreach (var file in vm.DescriptionImageFiles)
        //        {

        //            string newID = Guid.NewGuid().ToString();

        //            var filePath = await _fileService.SaveFileAsync(file, newID, "DescriptionImage");

        //            _context.DescriptionImage.Add(new DescriptionImage
        //            {
        //                DescriptionImageID = newID,
        //                ProgrammeID = programme.ProgrammeID,
        //                DescriptionImageName = file.FileName,
        //                ImagePath = filePath
        //            });
        //            count++;
        //        }
        //    }




        //    // 4. 更新場次與票區 (巢狀處理)
        //    foreach (var sessionVM in vm.Sessions)

        //    {
        //        var dbSession = programme.Session.FirstOrDefault(s => s.SessionID == sessionVM.SessionID);

        //        if (dbSession != null)
        //        {
        //            // 更新場次時間
        //            dbSession.StartTime = sessionVM.StartTime;
        //            dbSession.SaleStartTime = sessionVM.SaleStartTime;
        //            dbSession.SaleEndTime = sessionVM.SaleEndTime;

        //            // --- 處理票區同步 (重點) ---
        //            // A. 找出被刪除的 (DB 有但 VM 沒有)
        //            var vmAreaIds = sessionVM.TicketsArea.Select(a => a.TicketsAreaID).ToList();
        //            var areasToRemove = dbSession.TicketsArea.Where(a => !vmAreaIds.Contains(a.TicketsAreaID)).ToList();
        //            _context.TicketsArea.RemoveRange(areasToRemove);

        //            // B. 更新或新增
        //            foreach (var areaVM in sessionVM.TicketsArea)
        //            {
        //                var dbArea = dbSession.TicketsArea.FirstOrDefault(a => a.TicketsAreaID == areaVM.TicketsAreaID);
        //                if (dbArea != null)
        //                {
        //                    // 更新現有票區
        //                    dbArea.TicketsAreaName = areaVM.TicketsAreaName;
        //                    dbArea.Price = areaVM.Price;
        //                    dbArea.RowCount = areaVM.RowCount;
        //                    dbArea.SeatCount = areaVM.SeatCount;
        //                    dbArea.VenueID = vm.VenueID; // 或是 areaVM.VenueID
        //                }
        //                else
        //                {
        //                    string taid = await _iIDService.GetNextTicketsAreaID(dbSession.SessionID);
        //                    // 新增票區 (沒有 ID 的情況)
        //                    dbSession.TicketsArea.Add(new TicketsArea
        //                    {
        //                        TicketsAreaID = taid, // 或是你的編號邏輯
        //                        TicketsAreaName = areaVM.TicketsAreaName,
        //                        Price = areaVM.Price,
        //                        RowCount = areaVM.RowCount,
        //                        SeatCount = areaVM.SeatCount,
        //                        VenueID = vm.VenueID,
        //                        TicketsAreaStatusID = "A"
        //                    });
        //                }
        //            }
        //        }
        //        else
        //        {

        //            // 產生新的場次 ID
        //            string sid = await _iIDService.GetNextSessionID(programme.ProgrammeID);

        //            var newSession = new Session
        //            {
        //                SessionID = sid,
        //                ProgrammeID = programme.ProgrammeID, // 記得關聯活動 ID
        //                StartTime = sessionVM.StartTime,
        //                SaleStartTime = sessionVM.SaleStartTime,
        //                SaleEndTime = sessionVM.SaleEndTime,
        //                TicketsArea = new List<TicketsArea>()
        //            };

        //            foreach (var areaVM in sessionVM.TicketsArea)
        //            {
        //                // 為新場次下的每個新票區產生 ID
        //                string newTaid = await _iIDService.GetNextTicketsAreaID(sid);
        //                newSession.TicketsArea.Add(new TicketsArea
        //                {
        //                    TicketsAreaID = newTaid,
        //                    TicketsAreaName = areaVM.TicketsAreaName,
        //                    Price = areaVM.Price,
        //                    RowCount = areaVM.RowCount,
        //                    SeatCount = areaVM.SeatCount,
        //                    VenueID = vm.VenueID,
        //                    TicketsAreaStatusID = "A"
        //                });
        //            }
        //            var vmSessionIds = vm.Sessions.Select(s => s.SessionID).ToList();
        //            // 1. 找出要刪除的場次 ID
        //            var sessionsToRemove = programme.Session
        //                .Where(s => !vmSessionIds.Contains(s.SessionID))
        //                .ToList();
        //            if (sessionsToRemove.Any())
        //            {
        //                // 先手動刪除票區 (防止級聯刪除衝突)
        //                foreach (var session in sessionsToRemove)
        //                {
        //                    var areasToRemove = _context.TicketsArea.Where(a => a.SessionID == session.SessionID);
        //                    _context.TicketsArea.RemoveRange(areasToRemove);
        //                }
        //                _context.Session.RemoveRange(sessionsToRemove);
        //            }

        //            // 2. 針對這些要刪除的場次，先處理它們底下的票區
        //            foreach (var session in sessionsToRemove)
        //            {
        //                // 找出該場次目前在資料庫中的所有票區並刪除
        //                var areasToRemove = _context.TicketsArea.Where(a => a.SessionID == session.SessionID);
        //                _context.TicketsArea.RemoveRange(areasToRemove);
        //            }

        //            programme.Session.Add(newSession);
        //        }

        //        var sessionsToRemove = programme.Session.Where(s => !vmSessionIds.Contains(s.SessionID)).ToList();
        //        _context.ChangeTracker.Clear();
        //        _context.Session.RemoveRange(sessionsToRemove);
        //    }
        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        ModelState.AddModelError("", "存檔失敗，請檢查資料長度或關聯性。");
        //        ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceID", "PlaceName", vm.PlaceID);
        //        return View(vm);
        //    }
        //}
    }
}
