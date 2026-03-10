using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.DTOs.Question;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.Images;
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.ViewModel;
using TicketSalesSystem.ViewModel.Booking;
using TicketSalesSystem.ViewModel.Member;
using TicketSalesSystem.ViewModel.Order;
using TicketSalesSystem.ViewModel.Programme;
using TicketSalesSystem.ViewModel.Programme.ProgrammeAdminDetail;

namespace TicketSalesSystem.Service.User
{
    public class UserService : IUser
    {
        private readonly TicketsContext _context;
        private readonly IUserAccessorService _userAccessorService;
        private readonly IFileService _fileService;
        public UserService(TicketsContext context, IUserAccessorService userAccessorService, IFileService fileService)
        {
            _context = context;
            _userAccessorService = userAccessorService;
            _fileService = fileService;
        }

        private readonly string _baseUrl = "http://10.10.51.9:5098/Photos/CoverImage/";

        public string GetImageFullUrl(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return "";

            // 🚩 如果資料庫已經存了完整路徑，就不要再拼 _baseUrl
            if (fileName.StartsWith("http") || fileName.StartsWith("/Photos"))
            {
                return fileName.StartsWith("/") ? $"http://10.10.51.9:5098{fileName}" : fileName;
            }

            // 🚩 否則才進行拼接，並確保斜線只有一個
            return _baseUrl.TrimEnd('/') + "/" + fileName.TrimStart('/');
            
        }


        //所有活動清單
        public async Task<List<VMProgramme>> GetProgrammesALL()
        {
            return await _context.Programme
                .Where(p => p.ProgrammeStatusID == "O")
                .OrderByDescending(p => p.ProgrammeID)
                .Select(p => new VMProgramme
                {
                    ProgrammeID = p.ProgrammeID,
                    ProgrammeName = p.ProgrammeName,
                    // 🚩 這裡直接補全 URL，讓 Android 的 Glide 能直接用
                    CoverImage = _baseUrl + p.CoverImage,

                    PlaceID = p.PlaceID,
                    PlaceName = p.Place != null ? p.Place.PlaceName : "尚未公佈地點",

                    ProgrammeStatusID = p.ProgrammeStatusID ?? "O",
                    ProgrammeStatusName = p.ProgrammeStatus != null ? p.ProgrammeStatus.ProgrammeStatusName : "售票中",

                    Capacity = p.Session.SelectMany(s => s.TicketsArea).Sum(a => a.Capacity),
                    Remaining = p.Session.SelectMany(s => s.TicketsArea).Sum(a => a.Remaining),

                    StartTime = p.Session.OrderByDescending(s => s.StartTime).Select(s => (DateTime?)s.StartTime).FirstOrDefault(),
                    SaleStartTime = p.Session.OrderByDescending(s => s.StartTime).Select(s => s.SaleStartTime).FirstOrDefault(),
                    SessionID = p.Session.OrderByDescending(s => s.StartTime).Select(s => s.SessionID).FirstOrDefault() ?? ""
                }).ToListAsync();
        }

        //最新5筆公告
        public async Task<List<PublicNotice>> GetLatestFiveNoticesAsync()
        {
            var latestNews = await _context.PublicNotice
                .Where(p => p.PublicNoticeStatus == true)
                .OrderByDescending(n => n.CreatedTime)
                .Take(5)
                .ToListAsync();
            return latestNews;
        }

        //常見問題清單
        public async Task<List<FAQ>> GetFAQsAsync ()
        {
            var faqs = await _context.FAQ
               .Include(f => f.FAQPublishStatus)
               .Include(f => f.FAQType)
               .Where(f => f.FAQPublishStatusID == "Y") // 只顯示已發布的 FAQ                
               .ToListAsync();
            return faqs;
        }

        //我要發問
        public async Task<bool> CreateQuestionAsync(Question question, IFormFile? upload, string memberId)
        {
            // 1. 補足後端資訊
            question.QuestionID = Guid.NewGuid().ToString();
            question.CreatedTime = DateTime.Now;
            question.MemberID = memberId;

            // 2. 處理檔案上傳
            if (upload != null && upload.Length != 0)
            {
                string dbPath = await _fileService.SaveFileAsync(upload, "Questions");
                question.UploadFile = dbPath;
            }

            // 3. 寫入資料庫
            _context.Add(question);
            return await _context.SaveChangesAsync() > 0;
        }

        //問題清單
        public async Task<List<Question>> GetMemberQuestionsAsync(string memberID)
        {
            return await _context.Question
                .Include(q => q.QuestionType)
                .Include(q => q.Reply)
                .Where(q => q.MemberID == memberID)
                .OrderByDescending(q => q.CreatedTime)
                .ToListAsync();
        }

        //問題詳細資料(含回覆)
        public async Task<QuestionDetailDTO> GetQuestionDetailForUserAsync(string questionId, string memberId)
        {
            var question =await _context.Question
             .Where(q => q.QuestionID == questionId && q.MemberID == memberId)
             .Select(q => new QuestionDetailDTO
             {
                 OrderID= q.OrderID,
                 QuestionID = q.QuestionID,
                 QuestionTitle = q.QuestionTitle, // 假設你的實體欄位叫這個
                 QuestionDescription = q.QuestionDescription,
                 CreatedTime = q.CreatedTime,
                 UploadFile = q.UploadFile,
                 QuestionTypeName = q.QuestionType.QuestionTypeName,

                 // 映射回覆清單
                 Reply = q.Reply
                     .Where(r => r.EmployeeID != null)
                     .OrderBy(r => r.CreatedTime)
                     .Select(r => new ReplyDTO
                     {
                         ReplyStatusID = r.ReplyStatusID,                         
                         ReplyDescription = r.ReplyDescription,
                         ReplyStatusName = r.ReplyStatus.ReplyStatusName,
                         EmployeeName = r.Employee.Name,
                         CreatedTime = r.CreatedTime
                     }).ToList()
             })
             .FirstOrDefaultAsync();

            return question;
        }


        //會員基本資料更新
        public async Task<(bool success, string message)> UpdateMemberProfileAsync(VMMemberUserEdit vm)
        {
            var member = await _context.Member.FindAsync(vm.MemberID);

            // 🚩 邏輯抽離：手機重複檢查
            bool isTelExist = await _context.Member.AnyAsync(m => m.Tel == vm.Tel && m.MemberID != vm.MemberID);
            if (isTelExist) return (false, "此手機號碼已被使用");

            try
            {
                // 🚩 邏輯抽離：手機變更重設驗證狀態
                if (member.Tel != vm.Tel)
                {
                    member.IsPhoneVerified = false;
                }

                member.Address = vm.Address;
                member.Tel = vm.Tel;
                member.Email = vm.Email;

                await _context.SaveChangesAsync();
                return (true, "個人資料已更新");
            }
            catch (Exception ex)
            {
                return (false, "更新失敗：" + ex.Message);
            }
        }

        // 活動詳細資訊
        public async Task<VMProgrammeAdminDetail> GetProgrammesDetail(string id)
        {
            var p = await _context.Programme
                .Where(p => p.ProgrammeID == id)
                .Select(p => new VMProgrammeAdminDetail
                {
                    ProgrammeID = p.ProgrammeID,
                    ProgrammeName = p.ProgrammeName,
                    StatusName = p.ProgrammeStatus != null ? p.ProgrammeStatus.ProgrammeStatusName : "售票中",
                    PlaceName = p.Place != null ? p.Place.PlaceName : "地點未定",
                    ProgrammeDescription = p.ProgrammeDescription,
                    Notice = p.Notice,

                    // 🚩 補全圖片絕對路徑 (重要！)
                    CoverImage = _baseUrl + p.CoverImage,
                    SeatImage = _baseUrl + p.SeatImage,

                    // 🚩 處理所有場次 (Sessions)
                    Sessions = p.Session.Select(s => new VMSessionDetail
                    {
                        SessionID = s.SessionID,
                        StartTime = s.StartTime,
                        SaleStartTime = s.SaleStartTime,
                        // 假設 VMSessionDetail 裡有剩餘票數相關欄位，可在這計算

                        TicketsAreas = s.TicketsArea.Select(a => new VMAreaDetail
                        {
                            TicketsAreaID = a.TicketsAreaID,
                            TicketsAreaName = a.TicketsAreaName,
                            Price = a.Price,
                            Capacity = a.Capacity,
                            Remaining = a.Remaining
                        }).ToList()
                    }).ToList(),

                    // 🚩 處理介紹圖片清單
                    DescriptionImages = p.DescriptionImage.Select(di => new VMDescriptionImage
                    {
                        ImagePath = _baseUrl + di.ImagePath,
                        DescriptionImageName = di.DescriptionImageName
                    }).OrderBy(di => di.DescriptionImageName).ToList()

                }).FirstOrDefaultAsync();
            return p;
        }

        // 使用者訂單清單
        public async Task<IEnumerable<VMBookingDetailsResponse>> GetUserOrdersAsync(string memberID)
        {
            // 統一計算剩餘時間的基準
            var now = DateTime.Now;

            var orders = await _context.Order
                .AsNoTracking()
                .Include(o => o.OrderStatus)
                .Include(o => o.Session).ThenInclude(s => s.Programme).ThenInclude(p => p.Place)
                .Include(o => o.Tickets).ThenInclude(t => t.TicketsArea)
                .Where(o => o.MemberID == memberID)
                .OrderByDescending(o => o.OrderCreatedTime)
                .Select(o => new VMBookingDetailsResponse
                {
                    OrderID = o.OrderID,
                    ProgrammeName = o.Session.Programme.ProgrammeName,
                    StartTime = o.Session.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    PlaceName = o.Session.Programme.Place.PlaceName,
                    FinalAmount = o.Tickets.Sum(t => t.TicketsArea.Price),
                    OrderStatusID = o.OrderStatusID,
                    OrderStatusName = o.OrderStatus.OrderStatusName,
                    Seats = o.Tickets.Select(t => $"{t.RowIndex}排{t.SeatIndex}號").ToList(),
                    // 🚩 邏輯優化：根據每筆訂單的創建時間計算倒數 訂單付款倒數 (這是動態的，剩 10 分鐘//
                    RemainingSeconds = o.OrderStatusID == "P"
                        ? (int)Math.Max(0, (o.OrderCreatedTime.AddMinutes(10) - now).TotalSeconds)
                        : 0,
                    Success = o.OrderStatusID == "Y",
                    Message = o.OrderStatusID == "Y" ? "付款完成" : o.OrderStatusID == "N" ? "訂單失效" : "待付款",
                    TicketDetails = o.Tickets.Select(t => new VMTicketDetail
                    {
                        SeatInfo = $"{t.TicketsArea.TicketsAreaName} {t.RowIndex}排 {t.SeatIndex}號",
                        StatusName = t.TicketsStatus.TicketsStatusName ?? "未知",
                        Price = t.TicketsArea.Price
                    }).ToList()
                })
                .ToListAsync();

            return orders;
        }

        // 使用者訂單詳細資訊
        public async Task<VMUserOrderDetail> GetUserOrderDetailAsync(string orderId)
        {
            var order = await _context.Order
                .AsNoTracking()
                .Include(o => o.OrderStatus)
                .Include(o => o.Session).ThenInclude(s => s.Programme).ThenInclude(s => s.Place)
                .Include(o => o.Tickets).ThenInclude(t => t.TicketsArea)
                .Include(o => o.Tickets).ThenInclude(t => t.Session)
                .Include(o => o.Tickets).ThenInclude(t => t.TicketsStatus)
                .Where(o => o.OrderID == orderId)
                .FirstOrDefaultAsync();

            var isPrintable = true;
            //取票天數
            //var isPrintable = DateTime.Now >= order.Session.StartTime.AddDays(-15);

            var vm = new VMUserOrderDetail
            {
                OrderID = order.OrderID,
                ProgrammeName = order.Session.Programme.ProgrammeName,
                StartTime = order.Session.StartTime.ToString("yyyy-MM-dd HH:mm"),
                PlaceName = order.Session.Programme.Place.PlaceName,
                FinalAmount = order.Tickets.Sum(t => t.TicketsArea.Price),
                OrderStatusName = order.OrderStatus.OrderStatusName,
                Seats = order.Tickets.Select(t => $"{t.RowIndex}排{t.SeatIndex}號").ToList(),
                IsPrintable = isPrintable,
                Tickets = order.Tickets.Select(t => new VMUserTicketItem
                {
                    OrderID = order.OrderID,
                    ProgrammeName = order.Session.Programme.ProgrammeName,
                    StartTime = order.Session.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    PlaceName = order.Session.Programme.Place.PlaceName,
                    FinalAmount = t.TicketsArea.Price,
                    TicketsID = t.TicketsID,
                    TicketsAreaName = t.TicketsArea.TicketsAreaName,
                    Seat = $"{t.RowIndex}排{t.SeatIndex}號",
                    CheckInCode = t.CheckInCode /*isPrintable? t.CheckInCode: null*/
                }).ToList()
            };
            return vm;
        }



    }
}
