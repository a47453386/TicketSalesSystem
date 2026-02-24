namespace TicketSalesSystem.Service.IUserAccessor
{
    public interface IUserAccessorService
    {
        string? GetMemberId();
        string? GetEmployeeId();


        string? GetUserRole();


        string? GetUserName();
        bool IsAuthenticated();
    }
}
