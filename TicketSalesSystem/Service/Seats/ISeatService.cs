using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel;
using TicketSalesSystem.ViewModel.Booking;

namespace TicketSalesSystem.Service.Seats
{
    public interface ISeatService
    {
        // 取得場次列表
        Task<List<Session>> GetSessionsAsync();

        // 同步票區狀態 (手動校正用，平時購票由 Trigger 自動處理)
        Task SyncAreaStatusAsync(string areaId);

        // 取得特定票區的座位圖
        Task<List<VMSeats>> GetAreaLayoutAsync(string areaId);

        // 取得場次的所有票區
        Task<IEnumerable<object>> GetAreasBySession(string sessionId);

        // 建立訂單與票券 (核心購票方法)
        Task<VMBookingResponse> CreateOrderAndTicketsAsync(VMBookingRequest request, string memberID);

    }
}
