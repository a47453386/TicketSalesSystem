using System.Security.Claims;

namespace TicketSalesSystem.Service.IUserAccessor
{
    public class UserAccessorService : IUserAccessorService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserAccessorService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // --- 會員專區 (MemberScheme) ---

        public bool IsMember() =>
            GetIdentity("MemberScheme")?.IsAuthenticated ?? false;

        public string? GetMemberId() =>
            GetIdentity("MemberScheme")?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // --- 員工專區 (EmployeeScheme) ---

        public bool IsEmployee()
        {
            var identity = GetIdentity("EmployeeScheme");
            if (identity == null || !identity.IsAuthenticated) return false;

            var role = identity.FindFirst(ClaimTypes.Role)?.Value;
            string[] staffRoles = { "S", "A", "B", "C", "F" };
            return staffRoles.Contains(role);
        }

        public string? GetEmployeeId() =>
            GetIdentity("EmployeeScheme")?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public string? GetUserRole() =>
            GetIdentity("EmployeeScheme")?.FindFirst(ClaimTypes.Role)?.Value;

        // ---通用工具 ---

        public string? GetUserName() =>
            _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        public bool IsAuthenticated() =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        // 核心私有方法：精確抓取某個口袋的證件
        private ClaimsIdentity? GetIdentity(string scheme) =>
            _httpContextAccessor.HttpContext?.User.Identities
                .FirstOrDefault(i => i.AuthenticationType == scheme);
    }
}