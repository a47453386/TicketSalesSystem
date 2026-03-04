using Humanizer;
using Microsoft.AspNetCore.Authorization;
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
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.Service.Validation.IProgrammeValidationService;
using TicketSalesSystem.ViewModel;
using TicketSalesSystem.ViewModel.Programme.CreateProgramme.CreateProgrammeStep;
using TicketSalesSystem.ViewModel.Programme.CreateProgramme.Item;
using TicketSalesSystem.ViewModel.Programme.EditProgramme;

namespace TicketSalesSystem.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(AuthenticationSchemes = "EmployeeScheme", Roles = "S,A,B")]
    [Route("ProgrammeDTO/[action]")]
    public class ProgrammeDTOController : Controller
    {
        private readonly IFileService _fileService;
        private readonly TicketsContext _context;
        private readonly IIDService _iIDService;
        private readonly IProgrammeService _programmeService;
        private readonly IProgrammeValidationService _programmeValidationService;
        private readonly IUserAccessorService _userAccessorService;

        public ProgrammeDTOController(IFileService fileService, TicketsContext context,
            IIDService iIDService, IProgrammeService programmeService,
            IProgrammeValidationService programmeValidationService,IUserAccessorService userAccessorService)
        {

            _fileService = fileService;
            _context = context;
            _iIDService = iIDService;
            _programmeService = programmeService;
            _programmeValidationService = programmeValidationService;
            _userAccessorService = userAccessorService;
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
            dto.Notice=vm.Notice;
            dto.PurchaseReminder=vm.PurchaseReminder;
            dto.CollectionReminder=vm.CollectionReminder;
            dto.RefundPolicy=vm.RefundPolicy;
            dto.LimitPerOrder = vm.LimitPerOrder;
            dto.OnShelfTime = vm.OnShelfTime;
            dto.PlaceID = vm.PlaceID;
            dto.ProgrammeStatusID = "D"; // 預設草稿
        }
        private void MapStep1ToVM(ProgrammeDTO dto, VMProgrammeStep1 vm)
        {
            vm.ProgrammeName = dto.ProgrammeName;
            vm.ProgrammeDescription = dto.ProgrammeDescription;
            vm.Notice = dto.Notice;
            vm.PurchaseReminder = dto.PurchaseReminder;
            vm.CollectionReminder = dto.CollectionReminder;
            vm.RefundPolicy = dto.RefundPolicy;
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
                .Select(s => new VMSessionItemForGrammeIndex
                {
                    SaleStartTime = s.SaleStartTime,
                    SaleEndTime = s.SaleEndTime,
                    StartTime = s.StartTime
                })
                .ToList() ?? new List<VMSessionItemForGrammeIndex>();
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
                var vmF = new VMSessionItemForGrammeIndex
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
            dto.VenueID = null;//因為場地已經下放到票區，頂層 VenueID 設為 null 或清空
            dto.TicketsAreaStatusID = vm.TicketsAreaStatusID;


            if (vm.Session != null && dto.Session != null)
            {
                for (int i = 0; i < vm.Session.Count; i++)
                {
                    var sessionVM = vm.Session[i];
                    // 依照順序取回 DTO 裡的場次 (因為 Step 2 到 Step 3 順序是一致的)
                    if (i < dto.Session.Count)
                    {
                        var targetSession = dto.Session[i];

                        // 更新場次的 VenueID (如果你 SessionDTO 也有這個欄位的話)
                        targetSession.VenueID = sessionVM.VenueID!;

                        // 🚩 3. 核心：將票區資料轉換並存入
                        targetSession.TicketsArea = sessionVM.TicketsArea.Select(ta => new TicketsAreaDTO
                        {
                            TicketsAreaID = ta.TicketsAreaID!,
                            TicketsAreaName = ta.TicketsAreaName,
                            RowCount = ta.RowCount,
                            SeatCount = ta.SeatCount,
                            Price = ta.Price,
                            Capacity = ta.RowCount * ta.SeatCount,
                            Remaining = ta.RowCount * ta.SeatCount,
                            VenueID = ta.VenueID!, // 🚩 確保每一列選的 VenueID 都有存進去
                            TicketsAreaStatusID = ta.TicketsAreaStatusID ?? "A"
                        }).ToList();
                    }
                }
            }

            // 🚩 4. 彙整所有票區名稱 (給發布預覽用)
            dto.TicketsArea = dto.Session!.SelectMany(s => s.TicketsArea)
                                .GroupBy(x => x.TicketsAreaName)
                                .Select(g => g.First())
                                .ToList();
        }
        private void MapStep3ToVM(ProgrammeDTO dto, VMProgrammeStep3 vm)
        {
            //映射頂層狀態
            vm.TicketsAreaStatusID = dto.TicketsAreaStatusID ?? "A";

            //vm.VenueID 已經下放到票區，所以這裡不需賦值給頂層 VenueID

            if (dto.Session != null && dto.Session.Any())
            {
                // 2. 映射場次列表
                vm.Session = dto.Session.Select(s => new VMSessionItemForGrammeIndex
                {
                    SessionID = s.SessionID,
                    StartTime = s.StartTime,
                    SaleStartTime = s.SaleStartTime,
                    SaleEndTime = s.SaleEndTime,
                    VenueID = s.VenueID,

                    // 3. 映射該場次下的票區列表
                    TicketsArea = s.TicketsArea?.Select(ta => new VMTicketsAreaItem
                    {
                        TicketsAreaID = ta.TicketsAreaID,
                        TicketsAreaName = ta.TicketsAreaName,
                        RowCount = ta.RowCount,
                        SeatCount = ta.SeatCount,
                        Price = ta.Price,

                        //將 DTO 裡每個票區獨立的 VenueID 帶回 VM，讓下拉選單正確選中
                        VenueID = ta.VenueID,

                        //即時計算：讓 View 載入時就能看到總數
                        Capacity = ta.RowCount * ta.SeatCount,
                        Remaining = ta.Remaining > 0 ? ta.Remaining : (ta.RowCount * ta.SeatCount),

                        TicketsAreaStatusID = ta.TicketsAreaStatusID ?? "A"
                    }).ToList() ?? new List<VMTicketsAreaItem>()
                }).ToList();
            }
        }
        [HttpGet]
        public async Task<IActionResult> CreateStep3()
        {
            // 1. 取得現有的 DTO (這是在 Step1 & Step2 存好的)
            var dto = GetCurrentDTO();
            if (dto == null) return RedirectToAction("CreateStep1");

            // 2. 建立 VM 並映射資料
            var vm = new VMProgrammeStep3();
            MapStep3ToVM(dto, vm);

            // 🚩 3. 準備「該場地專屬」的區域下拉選單
            await LoadVenueListAsync(dto.PlaceID, dto.VenueID);

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStep3(VMProgrammeStep3 vm)
        {
            // 1. 取得 Session 中的 DTO，確保流程沒中斷
            var dto = GetCurrentDTO();
            if (dto == null) return RedirectToAction("CreateStep1");

            //  2. 手動排除不需要的驗證項
            // 如果 VM 頂層有 VenueID 屬性但此步驟沒用到，需排除以免 ModelState.IsValid 永遠為 false
            ModelState.Remove("VenueID");

            // 3. 核心業務邏輯驗證
            if (vm.Session != null)
            {
                // 效能優化：一次抓出該場地 (PlaceID) 下的所有物理區域，存入 Dictionary
                // 這樣在後面的嵌套迴圈中就不用重複讀取資料庫
                var allVenues = _context.Venue
                    .Where(v => v.PlaceID == dto.PlaceID)
                    .ToDictionary(v => v.VenueID);

                foreach (var session in vm.Session)
                {
                    // A. 場次完整性檢查
                    if (session.TicketsArea == null || !session.TicketsArea.Any())
                    {
                        ModelState.AddModelError("", $"場次【{session.StartTime:yyyy-MM-dd HH:mm}】請至少新增一個票區");
                        continue;
                    }

                    foreach (var area in session.TicketsArea)
                    {
                        // B. 檢查是否選取區域 ID
                        if (string.IsNullOrEmpty(area.VenueID))
                        {
                            ModelState.AddModelError("", "請確保每個票區都已指定對應的場館區域");
                            continue;
                        }

                        // C. 從 Dictionary 抓取物理設定，並驗證該區域是否屬於該場地
                        if (!allVenues.TryGetValue(area.VenueID, out var physicalVenue))
                        {
                            ModelState.AddModelError("", $"區域編號 {area.VenueID} 不屬於所選場地或已不存在");
                            continue;
                        }

                        // D. 區域狀態驗證：必須為 "A" (可用)
                        if (physicalVenue.VenueStatusID != "A")
                        {
                            ModelState.AddModelError("", $"區域【{physicalVenue.VenueName}】目前處於維修或停用狀態，無法設定票區");
                        }

                        // E. 物理容量驗證：計算設定的總位數是否超過場館原始物理上限
                        int physicalCapacity = physicalVenue.RowCount * physicalVenue.SeatCount;
                        int requestedCapacity = area.RowCount * area.SeatCount;

                        if (requestedCapacity > physicalCapacity)
                        {
                            ModelState.AddModelError("",
                                $"區域【{physicalVenue.VenueName}】物理上限為 {physicalCapacity} " +
                                $"(原廠設定 {physicalVenue.RowCount}排 x {physicalVenue.SeatCount}座)，" +
                                $"您的設定 ({requestedCapacity}) 已超出限制");
                        }
                    }
                }
            }

            // 4. 驗證失敗處理
            if (!ModelState.IsValid)
            {
                // 重新準備下拉選單資料，否則 View 會因為 ViewBag.VenueList 為 null 而崩潰
               await LoadVenueListAsync(dto.PlaceID, dto.VenueID);
                return View(vm);
            }

            // 5. 驗證成功：映射資料並存入 DTO
            // 這裡會將 VM 裡面的多場次、多票區資料寫入 DTO
            MapStep3ToDo(vm, dto);

            // 6. 存回 Session 持久化
            SaveDTO(dto);

            // 7. 前往下一步：圖片上傳與詳細描述
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
                string extension = Path.GetExtension(file.FileName); // 拿 .jpg
                string newID = Guid.NewGuid().ToString(); // 拿 GUID 前八碼
                string newFileName = newID + extension; // 變成 8a2b3c4d.jpg

                //呼叫多載A
                var savedPath = await _fileService.SaveFileAsync(file, newID, "DescriptionImage");
                dto.DescriptionImage.Add(new DescriptionImageDTO
                {
                    DescriptionImageID = newID,
                    DescriptionImageName = newFileName,
                    TempUrl = savedPath
                });
            }

            SaveDTO(dto);
            return RedirectToAction("CreateConfirm");
        }

        
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

                if (dto.Session != null)
                {
                    foreach (var session in dto.Session)
                    {
                        if (session.TicketsArea != null)
                        {
                            foreach (var area in session.TicketsArea)
                            {
                                // 根據 VenueID 找出名稱
                                area.VenueName = _context.Venue.Find(area.VenueID)?.VenueName ?? "未知區域";
                                area.AreaColor = _context.Venue.Find(area.VenueID)?.AreaColor ?? "#CCCCCC"; // 預設灰色
                            }
                        }
                    }
                }

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
            var employeeID = _userAccessorService.GetEmployeeId();
            var dto = GetCurrentDTO(); // 取得 Session 中的暫存資料

            if (dto == null || string.IsNullOrEmpty(dto.ProgrammeName))
            {
                return RedirectToAction("CreateStep1");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // --- 1. 建立活動本體 (Programme) ---
                    string pid = await _iIDService.GetNextProgrammeID();
                    if (string.IsNullOrEmpty(pid)) throw new Exception("無法生成活動 ID (pid)。");

                    var programme = new Programme
                    {
                        ProgrammeID = pid,
                        ProgrammeName = dto.ProgrammeName,
                        ProgrammeDescription = dto.ProgrammeDescription,
                        Notice = dto.Notice,
                        PurchaseReminder = dto.PurchaseReminder,
                        CollectionReminder = dto.CollectionReminder,
                        RefundPolicy = dto.RefundPolicy,
                        CreatedTime = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        CoverImage = dto.CoverImage,
                        SeatImage = dto.SeatImage,
                        LimitPerOrder = dto.LimitPerOrder,
                        OnShelfTime = dto.OnShelfTime,
                        ProgrammeStatusID = dto.ProgrammeStatusID ?? "D",
                        EmployeeID = employeeID,
                        PlaceID = dto.PlaceID
                    };

                    _context.Programme.Add(programme);
                    // 🚩 必須先存檔，後續的 SessionID 生成函數才能在 DB 裡找到這個 pid
                    await _context.SaveChangesAsync();

                    // --- 2. 處理場次 (Session) 與票區 (TicketsArea) ---
                    if (dto.Session != null)
                    {
                        foreach (var sDto in dto.Session)
                        {
                            // 生成場次 ID
                            string sid = await _iIDService.GetNextSessionID(pid);
                            if (string.IsNullOrWhiteSpace(sid)) throw new Exception($"場次 ID 生成失敗 (pid: {pid})");

                            // 🚩 修正點：將資料確實填入 Session 物件
                            var session = new Session
                            {
                                SessionID = sid.Trim(), // 確保去掉 char(10) 的空白
                                ProgrammeID = pid,
                                StartTime = sDto.StartTime,
                                SaleStartTime = sDto.SaleStartTime,
                                SaleEndTime = sDto.SaleEndTime
                            };

                            _context.Session.Add(session);
                            // 🚩 必須先存場次，後續的 TicketsArea 才能建立關聯
                            await _context.SaveChangesAsync();

                            // 處理該場次下的票區
                            if (sDto.TicketsArea != null)
                            {
                                foreach (var aDto in sDto.TicketsArea)
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
                                        // 計算總容量與初始餘額
                                        Capacity = aDto.RowCount * aDto.SeatCount,
                                        Remaining = aDto.RowCount * aDto.SeatCount,
                                        TicketsAreaStatusID = "A" // 預設為可用
                                    };
                                    _context.TicketsArea.Add(ticketsArea);
                                    // 存入該場次的所有票區
                                    await _context.SaveChangesAsync();
                                }
                                
                            }
                        }
                    }

                    // --- 3. 處理活動描述圖片 (DescriptionImage) ---
                    if (dto.DescriptionImage != null)
                    {
                        foreach (var diDto in dto.DescriptionImage)
                        {
                            var descriptionImage = new DescriptionImage
                            {
                                // 如果 ID 是自動生成的，這裡可以不填
                                DescriptionImageID = diDto.DescriptionImageID ?? Guid.NewGuid().ToString().Substring(0, 10),
                                ProgrammeID = pid,
                                DescriptionImageName = diDto.DescriptionImageName,
                                ImagePath = diDto.TempUrl
                            };
                            _context.DescriptionImage.Add(descriptionImage);
                            
                        }
                    }

                    // 最終統一存檔並提交事務
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // 清除 Session 並導向成功頁面
                    HttpContext.Session.Remove("programme");
                    return RedirectToAction("AdminIndex", "Programmes", new { msg = "活動部署完成，系統已啟動！" });

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "無底層訊息";

                    // 🚩 戰情室錯誤回報介面
                    return Content($"<div style='background:#000; color:#ff4d4d; padding:20px; font-family:monospace; border:2px solid #ff4d4d;'>" +
                                   $"<h3>[SYSTEM_DEPLOY_FAILED]</h3>" +
                                   $"<p>ERROR_MSG: {ex.Message}</p>" +
                                   $"<p>INNER_CAUSE: {innerMessage}</p>" +
                                   $"<p>STACK_TRACE: {ex.StackTrace}</p>" +
                                   $"</div>", "text/html");
                }
            }
        }




        [HttpGet]
        public async Task<IActionResult> GetVenues(string placeId)
        {
            var venues = await _context.Venue
                .Where(v => v.PlaceID == placeId)
                .Select(v => new {
                    VenueID = v.VenueID,   // 🚩 明確名稱
                    VenueName = v.VenueName,
                    RowCount = v.RowCount,
                    SeatCount = v.SeatCount,
                    AreaColor = v.AreaColor ?? "#00f2ff"
                })
                .ToListAsync();
            return Json(venues);
        }



        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if(id==null) return NotFound();

            var vm = await _programmeService.GetProgrammeForEditAsync(id);

            if (vm == null) return NotFound();

            await PrepareEditViewData(vm);

            return View(vm);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, VMProgrammeEdit vm)
        {
            // 🚩 修正：清除既有圖片的必填驗證，因為編輯時不會有 ImageFile
            // 清除 DescriptionImage[0].ImageFile, DescriptionImage[1].TempUrl 等路徑
            var imageValidationKeys = ModelState.Keys
                .Where(k => k.Contains("DescriptionImage") && (k.EndsWith("ImageFile") || k.EndsWith("TempUrl")));

            foreach (var key in imageValidationKeys)
            {
                ModelState.Remove(key);
            }

            // 🚩 同時清除「追加上傳」的新檔案驗證 (如果它是選填)
            ModelState.Remove("NewDescriptionFiles");

            if (id != vm.ProgrammeID) return NotFound();
            if (id != vm.ProgrammeID) return NotFound();

            // 🚩 1. 效能優化：一次抓出該場地 (PlaceID) 下所有的實體區域設定
            // 這包含了我們在 CreateStep3 用的 RowCount 與 SeatCount 物理上限
            var allPhysicalVenues = await _context.Venue
                 .Where(v => v.PlaceID == vm.PlaceID)
                 .ToDictionaryAsync(
                     v => v.VenueID,
                     v => v,
                     StringComparer.OrdinalIgnoreCase
                 );

            // 🚩 2. 業務邏輯驗證迴圈
            if (vm.Session != null)
            {
                foreach (var session in vm.Session)
                {
                    // A. 基本檢查：場次不可無票區
                    if (session.TicketsArea == null || !session.TicketsArea.Any())
                    {
                        ModelState.AddModelError("", $"場次【{session.StartTime:yyyy/MM/dd}】請至少保留一個票區。");
                        continue;
                    }

                    foreach (var area in session.TicketsArea)
                    {
                        // B. 驗證區域歸屬：確保選的 VenueID 真的屬於這個 Place
                        var venueId = area.VenueID?.Trim();
                        if (string.IsNullOrEmpty(venueId) || !allPhysicalVenues.TryGetValue(venueId, out var physical))
                        {
                            ModelState.AddModelError("", $"票區【{area.TicketsAreaName}】所選區域不屬於目前場地。");
                            continue;
                        }

                        // C. 驗證區域狀態：不可選擇維修中 (非 "A") 的區域
                        if (physical.VenueStatusID != "A")
                        {
                            ModelState.AddModelError("", $"區域【{physical.VenueName}】目前處於停用或維修狀態。");
                        }

                        // D. 驗證物理容量：不可超過場館原始設計的排數或座數
                        if (area.RowCount > physical.RowCount || area.SeatCount > physical.SeatCount)
                        {
                            ModelState.AddModelError("",
                                $"區域【{physical.VenueName}】物理上限為 {physical.RowCount}排 x {physical.SeatCount}座，您的設定已超出限制。");
                        }
                        
                        // E. 核心業務驗證：呼叫你的 ValidationService 檢查已售票券
                        // 確保縮減後的容量不會「壓碎」已經賣出去的票
                        var (isCapacityValid, capacityMsg) = await _programmeValidationService.ValidateAreaCapacityAsync(
                            area.TicketsAreaID,
                            area.RowCount,
                            area.SeatCount
                        );

                        if (!isCapacityValid)
                        {
                            ModelState.AddModelError("", capacityMsg);
                        }
                    }
                }
            }

            // 🚩 3. 驗證失敗處理
            if (!ModelState.IsValid)
            {
                // 必須重新填滿下拉選單，否則 View 的 SelectList 會變空
                await PrepareEditViewData(vm);
                return View(vm);
            }

            // 🚩 4. 執行資料庫事務 (Transaction)
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 抓取現有實體（包含所有關聯資料）
                    var programmeDb = await _context.Programme
                        .Include(p => p.Session).ThenInclude(s => s.TicketsArea)
                        .Include(p => p.DescriptionImage)
                        .FirstOrDefaultAsync(p => p.ProgrammeID == id);

                    if (programmeDb == null) return NotFound();

                    // 執行各項同步更新
                    // 1. 更新主檔
                    await _programmeService.UpdateProgrammeAsync(programmeDb, vm);

                    // 2. 同步說明圖片 (處理刪除舊圖與新增新圖)
                    await _programmeService.SyncImagesAsync(programmeDb, vm);

                    // 3. 同步場次與票區 (處理刪除、更新、與新增 ID 生成)
                    await _programmeService.SyncProgrammeDetailsAsync(programmeDb, vm);

                    // 存檔
                    await _context.SaveChangesAsync();

                    // 提交事務
                    await transaction.CommitAsync();

                    return RedirectToAction("AdminIndex", "Programmes");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // 捕捉 InnerException 取得更詳細的 DB 錯誤資訊 (如外鍵衝突)
                    var fullMessage = ex.InnerException?.Message ?? ex.Message;

                    // 回到編輯頁並顯示錯誤
                    ModelState.AddModelError("", $"系統更新失敗：{fullMessage}");
                    await PrepareEditViewData(vm);
                    return View(vm);
                }
            }
        }


        private async Task PrepareEditViewData(VMProgrammeEdit vm)
        {
            ViewData["PlaceID"] = new SelectList(_context.Place, "PlaceID", "PlaceName", vm.PlaceID);
            ViewData["ProgrammeStatusID"] = new SelectList(_context.ProgrammeStatus, "ProgrammeStatusID", "ProgrammeStatusName", vm.ProgrammeStatusID);

            await LoadVenueListAsync(vm.PlaceID);
        }

        private async Task LoadVenueListAsync(string placeId, string? selectedVenueId = null)
        {
            var venues = await _context.Venue.Where(v => v.PlaceID == placeId).ToListAsync();

            //給 View 頂部 @foreach 樣板用的 (List<Venue>)
            ViewBag.RawVenues = venues;

            //給 Razor asp-items 用的 (SelectList)
            ViewBag.VenueList = new SelectList(venues.Select(v => new {
                v.VenueID,
                DisplayName = v.VenueName + (v.VenueStatusID != "A" ? " (不可用)" : "")
            }), "VenueID", "DisplayName", selectedVenueId);

            
        }












    }
}
