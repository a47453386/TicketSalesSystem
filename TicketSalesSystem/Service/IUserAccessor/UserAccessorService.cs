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

        public string? GetUserName()
        {
            // 1. 先試著從會員口袋拿名字
            var memberName = GetIdentity("MemberScheme")?.Name;
            if (!string.IsNullOrEmpty(memberName)) return memberName;

            // 2. 如果會員沒登入，再從員工口袋拿名字
            var employeeName = GetIdentity("EmployeeScheme")?.Name;
            if (!string.IsNullOrEmpty(employeeName)) return employeeName;

            return null;
        }
        public string? GetOnlyMemberName()
        {
            var identity = _httpContextAccessor.HttpContext?.User.Identities
                .FirstOrDefault(i => i.AuthenticationType == "MemberScheme");

            return identity?.Name; // 如果沒登入會員，這裡會是 null
        }
        // 檢查是否「任何一個身分」有登入
        public bool IsAuthenticated()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Identities.Any(i => i.IsAuthenticated) ?? false;
        }

        // 核心私有方法：修正 AuthenticationType 的比對 (建議忽略大小寫)
        private ClaimsIdentity? GetIdentity(string scheme) =>
            _httpContextAccessor.HttpContext?.User.Identities
                .FirstOrDefault(i => string.Equals(i.AuthenticationType, scheme, StringComparison.OrdinalIgnoreCase));
    }
}