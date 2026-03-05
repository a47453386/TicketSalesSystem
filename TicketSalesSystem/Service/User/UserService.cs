using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.ViewModel;
using TicketSalesSystem.ViewModel.Booking;
using TicketSalesSystem.ViewModel.Programme;
using TicketSalesSystem.ViewModel.Programme.ProgrammeAdminDetail;

namespace TicketSalesSystem.Service.User
{
    public class UserService : IUser
    {
        private readonly TicketsContext _context;
        private readonly IUserAccessorService _userAccessorService;
        public UserService(TicketsContext context, IUserAccessorService userAccessorService)
        {
            _context = context;
            _userAccessorService = userAccessorService;
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
    }
}
