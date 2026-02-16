using TicketSalesSystem.DTOs;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel.Programme.CreateProgramme.CreateProgrammeStep;
using TicketSalesSystem.ViewModel.Programme.EditProgramme;

namespace TicketSalesSystem.Service.IProgramme
{
    public interface IProgrammeService
    {
        //// 新增 (POST)
        //Task CreateProgrammeAsync(VMProgrammeStep1 vm1, VMProgrammeStep2 vm2);





        // 取得資料用 (給 Edit 頁面初始顯示)
        Task<VMProgrammeEdit> GetProgrammeForEditAsync(string id);

        // 更新 (PUT)
        Task UpdateProgrammeAsync(string id, VMProgrammeEdit vm);

        // 場次票區更新
        Task SyncProgrammeDetailsAsync(string programmeID, VMProgrammeEdit vm);
        //圖片更新
        Task SyncImagesAsync(string programmeID, List<DescriptionImageDTO> image);
    }
}
