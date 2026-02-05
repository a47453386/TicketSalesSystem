using TicketSalesSystem.ViewModel;

namespace TicketSalesSystem.Service.Validation.NewFolder
{
    public interface IBookingValidationService
    {
        Task<(bool IsValid, string Message)> ValidateAllAsync(VMBookingRequest request);
    }
}
