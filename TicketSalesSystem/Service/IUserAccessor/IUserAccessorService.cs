namespace TicketSalesSystem.Service.IUserAccessor
{
    public interface IUserAccessorService
    {
        // --- 🏠 會員專區 (Member-Specific) ---
        
        bool IsMember();//判斷是否具備有效的會員身分 (MemberScheme)
        string? GetMemberId();//取得當前會員的唯一識別 ID


        // --- 🛡️ 員工專區 (Employee-Specific) ---
        
        bool IsEmployee();// 判斷是否具備有效的員工身分且屬於指定角色 (EmployeeScheme) 
        string? GetEmployeeId();// 取得當前員工的唯一識別 ID 
        string? GetUserRole();//取得當前員工的角色代號 (如: S, A, B...)


        // --- 🌐 通用工具 (General Tools) ---
        
        string? GetUserName();// 取得當前 Identity 的顯示名稱 (通常來自 ClaimTypes.Name)
        bool IsAuthenticated();// 只要具備任一已驗證的身分即回傳 true
    }
}