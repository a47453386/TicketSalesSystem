using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel.EditProgramme;

namespace TicketSalesSystem.Service.IProgramme
{
    public interface IProgrammeService
    {
        // 同步活動、場次與票區的核心邏輯
        Task SyncProgrammeDetailsAsync(Programme dbProgramme, VMProgrammeEdit vm);
        //圖片更新
        Task SyncImagesAsync(Programme dbProgramme, VMProgrammeEdit vm);
    }
}
