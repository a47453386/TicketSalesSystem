using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.Metrics;
using TicketSalesSystem.DTOs;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.ID;
using TicketSalesSystem.Service.Images;
using TicketSalesSystem.ViewModel;
using TicketSalesSystem.ViewModel.Programme.CreateProgramme.Item;
using TicketSalesSystem.ViewModel.Programme.EditProgramme;

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
                    Capacity=a.Capacity,
                    Remaining=a.Remaining,
                    VenueID =a.VenueID
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
        public async Task UpdateProgrammeAsync(Programme programme, VMProgrammeEdit vm)
        {

            //if (programmme == null) return;(控制邏輯)

            // 2.更新活動主檔
            programme.ProgrammeName = vm.ProgrammeName;
            programme.ProgrammeDescription = vm.ProgrammeDescription;
            programme.LimitPerOrder = vm.LimitPerOrder;
            programme.UpdatedAt=DateTime.Now;
            programme.CoverImage=vm.CoverImage;
            programme.SeatImage=vm.SeatImage;
            programme.LimitPerOrder=vm.LimitPerOrder;
            programme.OnShelfTime=vm.OnShelfTime;
            programme.PlaceID=vm.PlaceID;
            programme.ProgrammeStatusID=vm.ProgrammeStatusID;
            programme.EmployeeID = "A23025";

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
                 .Where(a => !string.IsNullOrEmpty(a.TicketsAreaID))
                 .Select(a => a.TicketsAreaID)
                 .ToList();

            //找出哪些該刪除：DB 有，但 VM 沒給            
            var toDelete = dbSession.TicketsArea
                .Where(s => !vmAreaIds.Contains(s.TicketsAreaID))
                .ToList();
            if (toDelete.Any())
            {
                foreach (var s in toDelete)
                {
                    //刪除該票區
                    _context.TicketsArea.RemoveRange(toDelete);
                }
            }

            string seedTaid = await _iIDService.GetNextTicketsAreaID(dbSession.SessionID);
            int startNumber = int.Parse(seedTaid.Substring(seedTaid.Length - 2));
            //更新與新增
            foreach (var vmArea in vmTicketsAreas)
            {
                int newCapacity = vmArea.RowCount * vmArea.SeatCount;
                if (string.IsNullOrEmpty(vmArea.TicketsAreaID))
                {
                    string newId = $"{dbSession.SessionID}{startNumber:D2}";
                    //新增全新場次
                    dbSession.TicketsArea.Add(new TicketsArea
                    {
                        TicketsAreaID = newId,
                        SessionID = dbSession.SessionID,
                        TicketsAreaName = vmArea.TicketsAreaName,
                        Price = vmArea.Price,
                        RowCount = vmArea.RowCount,
                        SeatCount = vmArea.SeatCount,
                        Capacity = newCapacity,
                        Remaining = newCapacity, // 新增票區，剩餘票數 = 總容量
                        VenueID = vmArea.VenueID,
                        TicketsAreaStatusID = "A",
                    });
                    startNumber++;
                }
                else
                {
                    //更新場次
                    var dbArea = dbSession.TicketsArea
                        .FirstOrDefault(a => a.TicketsAreaID == vmArea.TicketsAreaID);
                    if (dbArea != null)
                    {
                        // 計算已售出的票數，用來校準 Remaining
                        int soldCount = await _context.Tickets
                            .CountAsync(t => t.TicketsAreaID == dbArea.TicketsAreaID && t.Order.OrderStatusID != "N");
                        //手動更新欄位
                        dbArea.TicketsAreaName = vmArea.TicketsAreaName;
                        dbArea.Price = vmArea.Price;
                        dbArea.RowCount = vmArea.RowCount;
                        dbArea.SeatCount = vmArea.SeatCount;
                        dbArea.Capacity = newCapacity;
                        dbArea.Remaining = newCapacity - soldCount; ;
                        dbArea.VenueID = vmArea.VenueID;
                        

                        //// 使用 SetValues 自動同步屬性，避免手動賦值的麻煩－另一個寫法
                        //_context.Entry(dbArea).CurrentValues.SetValues(vmArea);                        
                    }
                }
            }



        }






        // 場次票區更新
        public async Task SyncProgrammeDetailsAsync(Programme programme, VMProgrammeEdit vm)
        {
           
            //找出哪些該刪除：DB 有，但 VM 沒給
            var vmSessionIDs= vm.Session
                .Select(s=>s.SessionID)
                .Where(id=>id!=null)
                .ToList();
            var toDelete = programme.Session
                .Where(s=>!vmSessionIDs.Contains(s.SessionID))
                .ToList();

            //刪除場次（EF 會根據 Cascade 規則連動刪除票區，若無，則需手動先刪票區）
            if (toDelete.Any())
            {
                _context.Session.RemoveRange(toDelete);

            }
            //更新與新增
            foreach(var vmSession in vm.Session)
            {
               
                if (string.IsNullOrEmpty(vmSession.SessionID))
                {
                    string sid = await _iIDService.GetNextSessionID(programme.ProgrammeID);

                    //取得該場次的第一個票區種子 ID (例如：202601010101)
                    string baseTaid = await _iIDService.GetNextTicketsAreaID(sid);

                    // 解析出最後兩位數字作為起始值， 取最後兩位並轉成數字
                    int startNumber = int.Parse(baseTaid.Substring(baseTaid.Length - 2));

                    //新增全新場次
                    var newSession = new Session
                    {
                        SessionID = sid,
                        ProgrammeID = programme.ProgrammeID,
                        StartTime = vmSession.StartTime,
                        SaleStartTime = vmSession.SaleStartTime,
                        SaleEndTime = vmSession.SaleEndTime,
                        TicketsArea = new List<TicketsArea>()
                    };

                    

                    foreach (var a in vmSession.TicketsArea)
                    {
                        string currentTaid = $"{sid}{startNumber:D2}";
                        newSession.TicketsArea.Add(new TicketsArea
                        {
                            TicketsAreaID = currentTaid,
                            TicketsAreaName = a.TicketsAreaName,
                            Price = a.Price,
                            RowCount = a.RowCount,
                            SeatCount = a.SeatCount,
                            Capacity = a.RowCount * a.SeatCount,
                            Remaining = a.RowCount * a.SeatCount,
                            VenueID = a.VenueID,
                            TicketsAreaStatusID = "A"
                        });
                        startNumber++;
                    }

                    _context.Session.Add(newSession);
                }
                else 
                { 
                    //更新場次
                    var dbSession= programme.Session
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
        public async Task SyncImagesAsync(Programme programme, List<DescriptionImageDTO> image)
        {
            //取得資料庫中該活動目前所有的說明圖片
            var dbImages = programme.DescriptionImage.ToList();

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
                            ProgrammeID = programme.ProgrammeID,
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
