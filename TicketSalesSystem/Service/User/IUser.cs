using TicketSalesSystem.ViewModel.Booking;
using TicketSalesSystem.ViewModel.Programme;
using TicketSalesSystem.ViewModel.Programme.ProgrammeAdminDetail;

namespace TicketSalesSystem.Service.User
{
    public interface IUser
    {
        
        //照片路徑
        string GetImageFullUrl(string fileName);

        //所有已開賣的活動
        Task<List<VMProgramme>> GetProgrammesALL();

        //活動詳細資料
        Task<VMProgrammeAdminDetail> GetProgrammesDetail(string id);

        //會員所有訂單

        Task<IEnumerable<VMBookingDetailsResponse>> GetUserOrdersAsync(string memberID);


    }
}
