namespace TicketSalesSystem.Service.Images
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string fileName, string folderName);
        Task<string> SaveFileAsync(IFormFile photo, string folderName);

        // 刪除實體檔案
        Task<bool> DeleteFileAsync(string fileName, string folderName);
    }
}
