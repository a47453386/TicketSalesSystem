using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.ViewModel.Programme;

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
    }
}
