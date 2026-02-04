using TicketSalesSystem.ViewModel;

namespace TicketSalesSystem.Service
{
    public interface ISeatService
    {
        //同步票區狀態(檢查是否售罄或需重新開放)
        Task SyncAreaStatusAaync(string areaId);

        //取得特定票區的座位圖
        Task<List<VMSeats>> GetAreaLayoutAsync(string areaId);

        //取得場次的所有票區
        Task<IEnumerable<object>> GetAreasBySession(string sessionId);

    }
}
