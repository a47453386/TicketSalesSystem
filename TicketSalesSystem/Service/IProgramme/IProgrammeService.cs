using TicketSalesSystem.DTOs;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel.CreateProgramme.CreateProgrammeStep;
using TicketSalesSystem.ViewModel.EditProgramme;

namespace TicketSalesSystem.Service.IProgramme
{
    public interface IProgrammeService
    {
        //// 新增 (POST)
        //Task CreateProgrammeAsync(VMProgrammeStep1 vm1, VMProgrammeStep2 vm2);





        // 取得資料用 (給 Edit 頁面初始顯示)
        Task<VMProgrammeEdit> GetProgrammeForEditAsync(string id);
                
        // 修改 (PUT) - 這是你現在要重寫的核心
        Task UpdateProgrammeAsync(string id, VMProgrammeEdit vm);

        // 核心同步邏輯 (這才是被重複呼叫的地方)
        Task SyncProgrammeDetailsAsync(string programmeID, VMProgrammeEdit vm);
        //圖片更新
        Task SyncImagesAsync(string programmeID, List<DescriptionImageDTO> image);
    }
}
