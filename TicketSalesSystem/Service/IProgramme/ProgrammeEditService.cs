using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.IdentityModel.Tokens;
using TicketSalesSystem.DTOs;
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
        

        public ProgrammeEditService(TicketsContext context, IIDService iIDService, IFileService fileService)
        {
            _context = context;
            _iIDService = iIDService;
            _fileService = fileService;
           
        }



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


        // 取得資料用 (給 Edit 頁面初始顯示)
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

            //if (programmme == null) return null;(控制邏輯)

            var vm = new VMProgrammeEdit();

            MapToVM(programmme, vm);

            return vm;
        }

        // 更新 (PUT)
        public async Task UpdateProgrammeAsync(string programmeID, VMProgrammeEdit vm)
        {
            // 1. 先抓取目前資料(含子層)
            var programmme = await GetProgrammeByIdAsync(programmeID);
            //if (programmme == null) return;(控制邏輯)
           
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

            ////3.處理圖片清單同步 (說明圖)
            //if(vm.DescriptionImage!=null)
            //{
            //    await 
            //}

            //// 4. 呼叫場次同步 (這裡面你可以繼續用你剛才寫的手動更新 Session)
            //await SyncProgrammeDetailsAsync(programmeID, vm);
            //// 5. 存檔
            //await _context.SaveChangesAsync();
        }




        // 票區更新
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
            var toDelete = dbSession.TicketsArea
                .Where(s => !vmSessionIDs.Contains(s.TicketsAreaID))
                .ToList();
            if (toDelete.Any())
            {
                foreach (var s in toDelete)
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






        // 場次票區更新
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
            var toDelete = programmme.Session
                .Where(s=>!vmSessionIDs.Contains(s.SessionID))
                .ToList();

            //只有在「真的被前端踢除」的情況下才刪除
            if(toDelete.Any())
            {
                foreach(var s in toDelete)
                {
                    //刪除該場次下所有票區
                    _context.Session.RemoveRange(toDelete);
                    _context.Session.Remove(s);
                }
            }
            //更新與新增
            foreach(var vmSession in vm.Session)
            {
               
                if (vmSession.SessionID == null)
                {
                    string sid = await _iIDService.GetNextSessionID(programmme.ProgrammeID);
                    string taid = await _iIDService.GetNextTicketsAreaID(sid);
                    //新增全新場次
                    var newSession = new Session
                    {
                        SessionID = sid,
                        ProgrammeID = programmme.ProgrammeID,
                        StartTime = vmSession.StartTime,
                        SaleStartTime = vmSession.SaleStartTime,
                        SaleEndTime = vmSession.SaleEndTime,
                        TicketsArea = vmSession.TicketsArea.Select(a => new TicketsArea
                        {
                            TicketsAreaID = taid, 
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

        // 圖片更新
        public async Task SyncImagesAsync(string programmeID, List<DescriptionImageDTO> image)
        {
            //取得資料庫中該活動目前所有的說明圖片
            var dbImages = await _context.DescriptionImage
                    .Where(i => i.ProgrammeID == programmeID)
                    .ToListAsync();

            //找出哪些該刪除：DB 有，但 VM 沒給
            var vmImageIds = image
                .Select(i => i.DescriptionImageID)
                .Where(id => id != null)
                .ToList();

            var toDelete = dbImages
                .Where(s => !vmImageIds.Contains(s.DescriptionImageID))
                .ToList();

            if (toDelete.Any())
            {
                foreach (var s in toDelete)
                {
                    bool isDeleted =await _fileService.DeleteFileAsync(s.DescriptionImageName, "DescriptionImage");
                    if (isDeleted)
                    {
                        _context.DescriptionImage.Remove(s);
                    }
                }

            }

            //更新與新增
            foreach(var imgVM in image)
            {
                if (imgVM.ImageFile != null && imgVM.ImageFile.Length > 0)
                {
                    //新增
                    string uploadedFileName = await _fileService.SaveFileAsync(imgVM.ImageFile, "DescriptionImage");

                    if (imgVM.DescriptionImageID == null)
                    {
                        var newDescriptionImage = new DescriptionImage
                        {
                            DescriptionImageID = Guid.NewGuid().ToString(),
                            DescriptionImageName = uploadedFileName,
                            ProgrammeID = programmeID,
                            ImagePath = uploadedFileName

                        };

                        _context.DescriptionImage.Add(newDescriptionImage);
                    }
                    else
                    {
                        var dbImage = dbImages.FirstOrDefault(i => i.DescriptionImageID == imgVM.DescriptionImageID);
                        if (dbImage != null)
                        {
                            await _fileService.DeleteFileAsync(dbImage.DescriptionImageName, "DescriptionImage");
                            dbImage.DescriptionImageName = uploadedFileName;
                            dbImage.ImagePath = uploadedFileName;
                        }
                    }

                }

            }


        }







    }
}
