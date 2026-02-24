using System.Security.Claims;

namespace TicketSalesSystem.Service.IUserAccessor
{
    public class UserAccessorService:IUserAccessorService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserAccessorService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetMemberId() =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        public string? GetEmployeeId() =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);


        // 抓取 Role (回傳 A, B, C, S)
        public string? GetUserRole() =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);



        public string? GetUserName() =>
            _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        public bool IsAuthenticated() =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

}
