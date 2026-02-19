using TicketSalesSystem.ViewModel.Booking;

namespace TicketSalesSystem.Service.Validation.NewFolder
{
    public interface IBookingValidationService
    {
        Task<(bool IsValid, string Message)> ValidateAllAsync(VMBookingRequest request, string memberId);
    }
}
