using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel;
using TicketSalesSystem.ViewModel.Booking;

namespace TicketSalesSystem.Service.Seats
{
    public interface ISeatService
    {
        //取得場次列表
        Task<List<Session>> GetSessionsAsync();

        //同步票區狀態(檢查是否售罄或需重新開放)
        Task SyncAreaStatusAaync(string areaId);

        //取得特定票區的座位圖
        Task<List<VMSeats>> GetAreaLayoutAsync(string areaId);

        //取得場次的所有票區
        Task<IEnumerable<object>> GetAreasBySession(string sessionId);

        //建立訂單與票券
        Task<VMBookingResponse> CreateOrderAndTicketsAsync(VMBookingRequest request, string memberID);

    }
}
