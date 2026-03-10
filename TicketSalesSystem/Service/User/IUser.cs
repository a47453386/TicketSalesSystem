using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.DTOs.Question;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel.Booking;
using TicketSalesSystem.ViewModel.Member;
using TicketSalesSystem.ViewModel.Order;
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

        //最新五則公告
        Task<List<PublicNotice>> GetLatestFiveNoticesAsync();

        //常見問題
        Task<List<FAQ>> GetFAQsAsync();

        //活動詳細資料
        Task<VMProgrammeAdminDetail> GetProgrammesDetail(string id);

        //我要發問
        Task<bool> CreateQuestionAsync(Question question, IFormFile? upload, string memberId);

        //問題詳細資料(含回覆)
        Task<QuestionDetailDTO> GetQuestionDetailForUserAsync(string questionId, string memberId);

        //會員所有提問
        Task<List<Question>> GetMemberQuestionsAsync(string memberID);
        
        //會員基本資料更新
        Task<(bool success, string message)> UpdateMemberProfileAsync(VMMemberUserEdit vm);


        //會員所有訂單
        Task<IEnumerable<VMBookingDetailsResponse>> GetUserOrdersAsync(string memberID);

        //會員訂單詳細資料
        Task<VMUserOrderDetail> GetUserOrderDetailAsync(string orderId);


    }
}
