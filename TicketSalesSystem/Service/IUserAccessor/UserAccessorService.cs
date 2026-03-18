using Microsoft.AspNetCore.Authentication;
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
            // 1. 取得所有的 Identity (可能包含 Member 和 Employee)
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return false;

            // 2. 🚩 關鍵：明確尋找 AuthenticationType 為 "EmployeeScheme" 的那個身份
            var employeeIdentity = user.Identities
                .FirstOrDefault(i => i.AuthenticationType == "EmployeeScheme");

            // 3. 只有當「員工身份」存在且已通過驗證時，才去檢查 Role
            if (employeeIdentity != null && employeeIdentity.IsAuthenticated)
            {
                var role = employeeIdentity.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                // 你的權限代碼清單
                string[] staffRoles = { "S", "A", "B", "C", "F" };
                return staffRoles.Contains(role);
            }

            return false;
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
        public string? GetEmployeeName()
        {
            // 🚩 同樣的邏輯：指定找 EmployeeScheme
            var identity = _httpContextAccessor.HttpContext?.User.Identities
                .FirstOrDefault(i => i.AuthenticationType == "EmployeeScheme");

            // 返回該 Identity 裡的 Name Claim
            return identity?.Name;
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