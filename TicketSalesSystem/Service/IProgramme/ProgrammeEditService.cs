using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.IdentityModel.Tokens;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.ID;
using TicketSalesSystem.Service.Images;
using TicketSalesSystem.ViewModel;
using TicketSalesSystem.ViewModel.CreateProgramme.Item;
using TicketSalesSystem.ViewModel.EditProgramme;

namespace TicketSalesSystem.Service.IProgramme
{
    public class ProgrammeEditService:IProgrammeService
    {
        private readonly TicketsContext _context;
        private readonly IIDService _iIDService;
        private readonly IFileService _fileService;

        private void MapToVM(Programme db, VMProgrammeEdit vm)
        {
            vm.ProgrammeID=db.ProgrammeID;
            vm.ProgrammeName=db.ProgrammeName;
            vm.ProgrammeDescription=db.ProgrammeDescription;
            vm.CoverImage=db.CoverImage;
            vm.SeatImage=db.SeatImage;
            vm.LimitPerOrder=db.LimitPerOrder;
            vm.OnShelfTime=db.OnShelfTime;
            vm.PlaceID=db.PlaceID;
            vm.ProgrammeStatusID=db.ProgrammeStatusID;
            

            vm.Session=db.Session.Select(s=>new VMSessionItem
            {
                SessionID=s.SessionID,
                StartTime=s.StartTime,
                SaleStartTime=s.SaleStartTime,
                SaleEndTime=s.SaleEndTime,
                TicketsArea=s.TicketsArea.Select(a=>new VMTicketsAreaItem
                {
                    TicketsAreaID=a.TicketsAreaID,
                    TicketsAreaName=a.TicketsAreaName,
                    Price=a.Price,
                    RowCount=a.RowCount,
                    SeatCount=a.SeatCount,
                    VenueID=a.VenueID
                }).ToList()
            }).ToList();

        }

        private async Task<Programme> GetProgrammeByIdAsync(string id)
        {
            var programmme = await _context.Programme
                .Include(p => p.Session)
                    .ThenInclude(s => s.TicketsArea)
                .Include(p => p.DescriptionImage)
                .FirstOrDefaultAsync(p => p.ProgrammeID == id);

            return programmme;
        }

        //將資料庫的資料撈出來，轉成VM
        public async Task<VMProgrammeEdit> GetProgrammeForEditAsync(string id)
        {
            var programmme = await GetProgrammeByIdAsync(id);

            if (programmme == null) return null;

            var vm = new VMProgrammeEdit();

            MapToVM(programmme, vm);

            return vm;
        }


        public async Task UpdateProgrammeAsync(string programmeID, VMProgrammeEdit vm)
        {
            // 1. 先抓取目前資料(含子層)
            var programmme = await GetProgrammeByIdAsync(programmeID);
            if (programmme == null) return;

            // 2.更新活動主檔
            programmme.ProgrammeName = vm.ProgrammeName;
            programmme.ProgrammeDescription = vm.ProgrammeDescription;
            programmme.LimitPerOrder = vm.LimitPerOrder;
            programmme.UpdatedAt=DateTime.Now;
            programmme.CoverImage=vm.CoverImage;
            programmme.SeatImage=vm.SeatImage;
            programmme.LimitPerOrder=vm.LimitPerOrder;
            programmme.OnShelfTime=vm.OnShelfTime;
            programmme.PlaceID=vm.PlaceID;
            programmme.ProgrammeStatusID=vm.ProgrammeStatusID;
            programmme.EmployeeID = "A23025";



        }





        private async Task SyncTicketsAreasAsync(Session dbSession, List<VMTicketsAreaItem> vmTicketsAreas)
        {
            //取得前端傳過來的所有 票區ID (排除新票區)
            var vmAreaIds = vmTicketsAreas
                .Where(a => a != null)
                .Select(a => a.TicketsAreaID)
                .ToList();

            //找出哪些該刪除：DB 有，但 VM 沒給
            var vmSessionIDs = dbSession.TicketsArea
                .Select(s => s.TicketsAreaID)
                .Where(id => id != null)
                .ToList();
            var toDelere = dbSession.TicketsArea
                .Where(s => !vmSessionIDs.Contains(s.TicketsAreaID))
                .ToList();
            if (toDelere.Any())
            {
                foreach (var s in toDelere)
                {
                    //刪除該票區
                    _context.TicketsArea.RemoveRange(s);
                    _context.TicketsArea.Remove(s);
                }
            }

            //更新與新增
            foreach (var vmArea in vmTicketsAreas)
            {

                if (vmArea.TicketsAreaID == null)
                {
                    //新增全新場次
                    var newArea = new TicketsArea
                    {
                        TicketsAreaID = await _iIDService.GetNextTicketsAreaID(dbSession.SessionID),
                        SessionID = dbSession.SessionID,
                        TicketsAreaName = vmArea.TicketsAreaName,
                        Price = vmArea.Price,
                        RowCount = vmArea.RowCount,
                        SeatCount = vmArea.SeatCount,
                        VenueID = vmArea.VenueID,
                        TicketsAreaStatusID = "A",
                    };
                    _context.TicketsArea.Add(newArea);

                }
                else
                {
                    //更新場次
                    var dbArea = dbSession.TicketsArea
                        .FirstOrDefault(a => a.TicketsAreaID == vmArea.TicketsAreaID);
                    if (dbArea != null)
                    {
                        //手動更新欄位
                        dbArea.TicketsAreaName = vmArea.TicketsAreaName;
                        dbArea.Price = vmArea.Price;
                        dbArea.RowCount = vmArea.RowCount;
                        dbArea.SeatCount = vmArea.SeatCount;
                        dbArea.VenueID = vmArea.VenueID;
                        dbArea.TicketsAreaStatusID = "A";

                        //// 使用 SetValues 自動同步屬性，避免手動賦值的麻煩－另一個寫法
                        //_context.Entry(dbArea).CurrentValues.SetValues(vmArea);                        
                    }
                }
            }



        }


       

       


        public async Task SyncProgrammeDetailsAsync(string programmeID, VMProgrammeEdit vm)
        {
            //抓取目前資料(含子層)
            var programmme = await GetProgrammeByIdAsync(programmeID);
            if (programmme == null) return ;

            //找出哪些該刪除：DB 有，但 VM 沒給
            var vmSessionIDs= vm.Session
                .Select(s=>s.SessionID)
                .Where(id=>id!=null)
                .ToList();
            var toDelere=programmme.Session
                .Where(s=>!vmSessionIDs.Contains(s.SessionID))
                .ToList();

            //只有在「真的被前端踢除」的情況下才刪除
            if(toDelere.Any())
            {
                foreach(var s in toDelere)
                {
                    //刪除該場次下所有票區
                    _context.Session.RemoveRange(toDelere);
                    _context.Session.Remove(s);
                }
            }
            //更新與新增
            foreach(var vmSession in vm.Session)
            {
                string taid = await _iIDService.GetNextTicketsAreaID("你的SID");
                if (vmSession.SessionID == null)
                {
                    //新增全新場次
                    var newSession = new Session
                    {
                        SessionID = await _iIDService.GetNextSessionID(programmme.ProgrammeID),
                        ProgrammeID = programmme.ProgrammeID,
                        StartTime = vmSession.StartTime,
                        SaleStartTime = vmSession.SaleStartTime,
                        SaleEndTime = vmSession.SaleEndTime,
                        TicketsArea = vmSession.TicketsArea.Select(a => new TicketsArea
                        {
                            TicketsAreaID = taid, //暫時用"TEMP"當作SessionID，之後會修正
                            TicketsAreaName = a.TicketsAreaName,
                            Price = a.Price,
                            RowCount = a.RowCount,
                            SeatCount = a.SeatCount,
                            VenueID = a.VenueID,
                            TicketsAreaStatusID = "A"
                        }).ToList()
                    };
                        _context.Session.Add(newSession);
                    
                }
                else 
                { 
                    //更新場次
                    var dbSession= programmme.Session
                        .FirstOrDefault(s=>s.SessionID==vmSession.SessionID);
                    if(dbSession!=null)
                    {
                        //手動更新欄位
                        dbSession.StartTime=vmSession.StartTime;
                        dbSession.SaleStartTime=vmSession.SaleStartTime;
                        dbSession.SaleEndTime=vmSession.SaleEndTime;
                        //遞迴處理票區同步
                        await SyncTicketsAreasAsync(dbSession, vmSession.TicketsArea);
                    }
                }
            }
        }



        




        //public ProgrammeEditService(TicketsContext context, IIDService iIDService, IFileService fileService)
        //{
        //    _context = context;
        //    _iIDService = iIDService;
        //    _fileService = fileService;
        //}

        //public async Task SyncProgrammeDetailsAsync(Programme dbProgramme, VMProgrammeEdit vm)
        //{
        //    // --- 階段 1: 更新主表基本資料 ---
        //    var oldCover = dbProgramme.CoverImage;
        //    var oldSeat = dbProgramme.SeatImage;

        //    // 🚩 注意：SetValues 會覆蓋所有同名屬性
        //    _context.Entry(dbProgramme).CurrentValues.SetValues(vm);

        //    if (string.IsNullOrEmpty(vm.CoverImage)) dbProgramme.CoverImage = oldCover;
        //    if (string.IsNullOrEmpty(vm.SeatImage)) dbProgramme.SeatImage = oldSeat;

        //    // --- 階段 2: 處理場次 (Sessions) 的刪除 ---
        //    //var vmSessionIds = vm.Session.Where(s => !string.IsNullOrEmpty(s.SessionID)).Select(s => s.SessionID).ToList();
        //    //var sessionsToRemove = dbProgramme.Session.Where(s => !vmSessionIds.Contains(s.SessionID)).ToList();

        //    //foreach (var s in sessionsToRemove)
        //    //{
        //    //    // 刪除該場次下所有票區
        //    //    _context.TicketsArea.RemoveRange(s.TicketsArea);
        //    //    _context.Session.Remove(s);
        //    //}

        //    // --- 階段 3: 處理場次的更新與新增 ---
        //    foreach (var sessionVM in vm.Session)
        //    {
        //        var dbSession = dbProgramme.Session.FirstOrDefault(s => s.SessionID == sessionVM.SessionID);

        //        if (dbSession != null)
        //        {
        //            // ✅ 改用手動更新場次屬性，避免 SetValues 觸發導覽屬性的自動追蹤衝突
        //            dbSession.StartTime = sessionVM.StartTime;
        //            dbSession.SaleStartTime = sessionVM.SaleStartTime;
        //            dbSession.SaleEndTime = sessionVM.SaleEndTime;

        //            // 遞迴處理票區同步
        //            await SyncTicketsAreasAsync(dbSession, sessionVM, vm.VenueID);
        //        }
        //        else
        //        {
        //            // 新增全新場次
        //            await AddNewSessionAsync(dbProgramme, sessionVM, vm.VenueID);
        //        }
        //    }
        //}

        //private async Task SyncTicketsAreasAsync(Session dbSession, VMSessionItem sessionVM, string defaultVenueID)
        //{
        //    // 1. 取得前端傳回來的有效 ID
        //    var vmAreaIds = sessionVM.TicketsArea
        //        .Where(a => !string.IsNullOrEmpty(a.TicketsAreaID))
        //        .Select(a => a.TicketsAreaID)
        //        .ToList();

        //    // 2. 刪除已移除的票區
        //    var areasToRemove = dbSession.TicketsArea
        //        .Where(a => !vmAreaIds.Contains(a.TicketsAreaID))
        //        .ToList();

        //    if (areasToRemove.Any())
        //    {
        //        _context.TicketsArea.RemoveRange(areasToRemove);
        //    }

        //    // 3. 更新或新增票區
        //    foreach (var areaVM in sessionVM.TicketsArea)
        //    {
        //        if (!string.IsNullOrEmpty(areaVM.TicketsAreaID))
        //        {
        //            // 🚩 這裡使用 Local 查找，避免去 Database 觸發新的追蹤
        //            var dbArea = dbSession.TicketsArea
        //                .FirstOrDefault(a => a.TicketsAreaID == areaVM.TicketsAreaID);

        //            if (dbArea != null)
        //            {
        //                // 手動更新欄位，不要用 SetValues
        //                dbArea.TicketsAreaName = areaVM.TicketsAreaName;
        //                dbArea.Price = areaVM.Price;
        //                dbArea.RowCount = areaVM.RowCount;
        //                dbArea.SeatCount = areaVM.SeatCount;
        //                dbArea.VenueID = areaVM.VenueID ?? defaultVenueID;
        //            }
        //            else
        //            {
        //                // 🚩 萬一 dbSession 沒 Include 到，但 Context 裡卻有
        //                // 嘗試從 Context.Local 找，並強制讓它跟 dbSession 關聯
        //                var orphanArea = _context.TicketsArea.Local
        //                    .FirstOrDefault(a => a.TicketsAreaID == areaVM.TicketsAreaID);

        //                if (orphanArea != null)
        //                {
        //                    _context.Entry(orphanArea).State = EntityState.Detached; // 強制斷開衝突者
        //                }
        //            }
        //        }
        //        else
        //        {
        //            // ✅ 新增
        //            string taid = await _iIDService.GetNextTicketsAreaID(dbSession.SessionID);
        //            dbSession.TicketsArea.Add(new TicketsArea
        //            {
        //                TicketsAreaID = taid,
        //                SessionID = dbSession.SessionID,
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

        //private async Task AddNewSessionAsync(Programme dbProgramme, VMSessionItem sessionVM, string defaultVenueID)
        //{
        //    string sid = await _iIDService.GetNextSessionID(dbProgramme.ProgrammeID);
        //    var newSession = new Session
        //    {
        //        SessionID = sid,
        //        ProgrammeID = dbProgramme.ProgrammeID,
        //        StartTime = sessionVM.StartTime,
        //        SaleStartTime = sessionVM.SaleStartTime,
        //        SaleEndTime = sessionVM.SaleEndTime,
        //        TicketsArea = new List<TicketsArea>() // 修正 NullReferenceException 的關鍵
        //    };

        //    foreach (var areaVM in sessionVM.TicketsArea)
        //    {
        //        string taid = await _iIDService.GetNextTicketsAreaID(sid);
        //        newSession.TicketsArea.Add(new TicketsArea
        //        {
        //            TicketsAreaID = taid,
        //            TicketsAreaName = areaVM.TicketsAreaName,
        //            Price = areaVM.Price,
        //            RowCount = areaVM.RowCount,
        //            SeatCount = areaVM.SeatCount,
        //            VenueID = areaVM.VenueID ?? defaultVenueID,
        //            TicketsAreaStatusID = "A"
        //        });
        //    }
        //    dbProgramme.Session.Add(newSession);
        //}

        //public async Task SyncImagesAsync(Programme dbProgramme, VMProgrammeEdit vm)
        //{
        //    // --- 1. 處理封面圖 (CoverImage) ---
        //    if (vm.CoverImageFile != null)
        //    {
        //        // 刪除舊檔案 (如果存在)
        //        if (!string.IsNullOrEmpty(dbProgramme.CoverImage))
        //        {
        //            _fileService.DeleteFileAsync(dbProgramme.CoverImage, "CoverImage");
        //        }
        //        // 存入新檔案：使用多載 B (自動產生 GUID 檔名)
        //        dbProgramme.CoverImage = await _fileService.SaveFileAsync(vm.CoverImageFile, "CoverImage");
        //    }

        //    // --- 2. 處理座位圖 (SeatImage) ---
        //    if (vm.SeatImageFile != null)
        //    {
        //        if (!string.IsNullOrEmpty(dbProgramme.SeatImage))
        //        {
        //            _fileService.DeleteFileAsync(dbProgramme.SeatImage, "SeatImage");
        //        }
        //        dbProgramme.SeatImage = await _fileService.SaveFileAsync(vm.SeatImageFile, "SeatImage");
        //    }

        //    // --- 3. 處理描述圖 (DescriptionImages) ---
        //    // A. 處理刪除：前端傳回來的 DeleteImageIds
        //    if (vm.DeleteImageIds != null && vm.DeleteImageIds.Any())
        //    {
        //        var toRemove = dbProgramme.DescriptionImage
        //            .Where(di => vm.DeleteImageIds.Contains(di.DescriptionImageID)).ToList();

        //        foreach (var img in toRemove)
        //        {
        //            _fileService.DeleteFileAsync(img.ImagePath, "DescriptionImage");
        //            _context.DescriptionImage.Remove(img);
        //        }
        //    }

        //    // B. 處理批次新增上傳
        //    if (vm.NewDescriptionFiles != null && vm.NewDescriptionFiles.Any())
        //    {
        //        foreach (var file in vm.NewDescriptionFiles)
        //        {
        //            var fileName = await _fileService.SaveFileAsync(file, "DescriptionImage");
        //            dbProgramme.DescriptionImage.Add(new DescriptionImage
        //            {
        //                ImagePath = fileName,
        //                ProgrammeID = dbProgramme.ProgrammeID
        //                // 如果 DescriptionImage 有專屬 ID 產製邏輯，記得在這裡呼叫 IIDService
        //            });
        //        }
        //    }
        //}

    }
}
