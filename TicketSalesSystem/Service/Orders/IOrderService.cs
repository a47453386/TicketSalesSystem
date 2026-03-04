using System.Threading.Tasks;
using TicketSalesSystem.ViewModel.Booking;

namespace TicketSalesSystem.Service.Orders
{
    public interface IOrderService
    {
        //獲取付款詳情
        Task<VMBookingResponse> GetPaymentDetailsAsync(string orderId);

        // 處理支付完成後的邏輯(正式結帳與電子票券核發)
        Task<(bool Success, string Message)> ProcessPaymentAsync(VMPaymentRequest request);
    }
}
