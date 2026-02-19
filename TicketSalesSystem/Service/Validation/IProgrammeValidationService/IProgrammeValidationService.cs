using TicketSalesSystem.ViewModel.Programme.EditProgramme;

namespace TicketSalesSystem.Service.Validation.IProgrammeValidationService
{
    public interface IProgrammeValidationService
    {
        // 驗證票區容量是否足以容納已售出的票
        Task<(bool IsValid, string Message)> ValidateAreaCapacityAsync(string areaId, int newRowCount, int newSeatCount);

        //// 也可以加入其他的驗證，例如驗證場次時間是否重疊
        //Task<(bool IsValid, string Message)> ValidateSessionTimeAsync(VMProgrammeEdit vm);
    }
}