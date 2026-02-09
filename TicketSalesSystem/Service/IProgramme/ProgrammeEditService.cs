using TicketSalesSystem.Models;
using TicketSalesSystem.Service.ID;
using TicketSalesSystem.Service.Images;
using TicketSalesSystem.ViewModel.CreateProgramme.Item;
using TicketSalesSystem.ViewModel.EditProgramme;

namespace TicketSalesSystem.Service.IProgramme
{
    public class ProgrammeEditService:IProgrammeService
    {
        private readonly TicketsContext _context;
        private readonly IIDService _iIDService;
        private readonly IFileService _fileService;

        public ProgrammeEditService(TicketsContext context, IIDService iIDService, IFileService fileService)
        {
            _context = context;
            _iIDService = iIDService;
            _fileService = fileService;
        }

        public async Task SyncProgrammeDetailsAsync(Programme dbProgramme, VMProgrammeEdit vm)
        {
            // --- 階段 1: 更新主表基本資料 ---
            var oldCover = dbProgramme.CoverImage;
            var oldSeat = dbProgramme.SeatImage;
            _context.Entry(dbProgramme).CurrentValues.SetValues(vm);

            // 強制還原圖片路徑（除非 vm 裡面有值）
            if (string.IsNullOrEmpty(vm.CoverImage)) dbProgramme.CoverImage = oldCover;
            if (string.IsNullOrEmpty(vm.SeatImage)) dbProgramme.SeatImage = oldSeat;

            // --- 階段 2: 處理場次 (Sessions) 的刪除 ---
            var vmSessionIds = vm.Session.Where(s => !string.IsNullOrEmpty(s.SessionID)).Select(s => s.SessionID).ToList();
            var sessionsToRemove = dbProgramme.Session.Where(s => !vmSessionIds.Contains(s.SessionID)).ToList();

            foreach (var s in sessionsToRemove)
            {
                // 必須先刪除子層票區，避免 FK 衝突
                _context.TicketsArea.RemoveRange(s.TicketsArea);
                _context.Session.Remove(s);
            }

            // --- 階段 3: 處理場次的更新與新增 ---
            foreach (var sessionVM in vm.Session)
            {
                var dbSession = dbProgramme.Session.FirstOrDefault(s => s.SessionID == sessionVM.SessionID);

                if (dbSession != null)
                {
                    // 更新現有場次
                    _context.Entry(dbSession).CurrentValues.SetValues(sessionVM);
                    // 遞迴處理票區同步
                    await SyncTicketsAreasAsync(dbSession, sessionVM, vm.VenueID);
                }
                else
                {
                    // 新增全新場次
                    await AddNewSessionAsync(dbProgramme, sessionVM, vm.VenueID);
                }
            }
        }

        private async Task SyncTicketsAreasAsync(Session dbSession, VMSessionItem sessionVM, string defaultVenueID)
        {
            var vmAreaIds = sessionVM.TicketsArea.Where(a => !string.IsNullOrEmpty(a.TicketsAreaID)).Select(a => a.TicketsAreaID).ToList();

            // 刪除前端已移除的票區
            var areasToRemove = dbSession.TicketsArea.Where(a => !vmAreaIds.Contains(a.TicketsAreaID)).ToList();
            _context.TicketsArea.RemoveRange(areasToRemove);

            // 更新或新增票區
            foreach (var areaVM in sessionVM.TicketsArea)
            {
                var dbArea = dbSession.TicketsArea.FirstOrDefault(a => a.TicketsAreaID == areaVM.TicketsAreaID);
                if (dbArea != null)
                {
                    _context.Entry(dbArea).CurrentValues.SetValues(areaVM);
                    dbArea.VenueID = areaVM.VenueID ?? defaultVenueID;
                }
                else
                {
                    // 新增票區
                    string taid = await _iIDService.GetNextTicketsAreaID(dbSession.SessionID);
                    dbSession.TicketsArea.Add(new TicketsArea
                    {
                        TicketsAreaID = taid,
                        TicketsAreaName = areaVM.TicketsAreaName,
                        Price = areaVM.Price,
                        RowCount = areaVM.RowCount,
                        SeatCount = areaVM.SeatCount,
                        VenueID = areaVM.VenueID ?? defaultVenueID,
                        TicketsAreaStatusID = "A"
                    });
                }
            }
        }

        private async Task AddNewSessionAsync(Programme dbProgramme, VMSessionItem sessionVM, string defaultVenueID)
        {
            string sid = await _iIDService.GetNextSessionID(dbProgramme.ProgrammeID);
            var newSession = new Session
            {
                SessionID = sid,
                ProgrammeID = dbProgramme.ProgrammeID,
                StartTime = sessionVM.StartTime,
                SaleStartTime = sessionVM.SaleStartTime,
                SaleEndTime = sessionVM.SaleEndTime,
                TicketsArea = new List<TicketsArea>() // 修正 NullReferenceException 的關鍵
            };

            foreach (var areaVM in sessionVM.TicketsArea)
            {
                string taid = await _iIDService.GetNextTicketsAreaID(sid);
                newSession.TicketsArea.Add(new TicketsArea
                {
                    TicketsAreaID = taid,
                    TicketsAreaName = areaVM.TicketsAreaName,
                    Price = areaVM.Price,
                    RowCount = areaVM.RowCount,
                    SeatCount = areaVM.SeatCount,
                    VenueID = areaVM.VenueID ?? defaultVenueID,
                    TicketsAreaStatusID = "A"
                });
            }
            dbProgramme.Session.Add(newSession);
        }

        public async Task SyncImagesAsync(Programme dbProgramme, VMProgrammeEdit vm)
        {
            // --- 1. 處理封面圖 (CoverImage) ---
            if (vm.CoverImageFile != null)
            {
                // 刪除舊檔案 (如果存在)
                if (!string.IsNullOrEmpty(dbProgramme.CoverImage))
                {
                    _fileService.DeleteFileAsync(dbProgramme.CoverImage, "CoverImage");
                }
                // 存入新檔案：使用多載 B (自動產生 GUID 檔名)
                dbProgramme.CoverImage = await _fileService.SaveFileAsync(vm.CoverImageFile, "CoverImage");
            }

            // --- 2. 處理座位圖 (SeatImage) ---
            if (vm.SeatImageFile != null)
            {
                if (!string.IsNullOrEmpty(dbProgramme.SeatImage))
                {
                    _fileService.DeleteFileAsync(dbProgramme.SeatImage, "SeatImage");
                }
                dbProgramme.SeatImage = await _fileService.SaveFileAsync(vm.SeatImageFile, "SeatImage");
            }

            // --- 3. 處理描述圖 (DescriptionImages) ---
            // A. 處理刪除：前端傳回來的 DeleteImageIds
            if (vm.DeleteImageIds != null && vm.DeleteImageIds.Any())
            {
                var toRemove = dbProgramme.DescriptionImage
                    .Where(di => vm.DeleteImageIds.Contains(di.DescriptionImageID)).ToList();

                foreach (var img in toRemove)
                {
                    _fileService.DeleteFileAsync(img.ImagePath, "DescriptionImage");
                    _context.DescriptionImage.Remove(img);
                }
            }

            // B. 處理批次新增上傳
            if (vm.NewDescriptionFiles != null && vm.NewDescriptionFiles.Any())
            {
                foreach (var file in vm.NewDescriptionFiles)
                {
                    var fileName = await _fileService.SaveFileAsync(file, "DescriptionImage");
                    dbProgramme.DescriptionImage.Add(new DescriptionImage
                    {
                        ImagePath = fileName,
                        ProgrammeID = dbProgramme.ProgrammeID
                        // 如果 DescriptionImage 有專屬 ID 產製邏輯，記得在這裡呼叫 IIDService
                    });
                }
            }
        }

    }
}
