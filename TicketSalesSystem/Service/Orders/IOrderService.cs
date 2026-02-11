using TicketSalesSystem.ViewModel.Booking;

namespace TicketSalesSystem.Service.Orders
{
    public interface IOrderService
    {
        // 處理支付完成後的邏輯
        Task<(bool Success, string Message)> ProcessPaymentAsync(VMPaymentRequest request);
    }
}
