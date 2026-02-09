
using System.Runtime.InteropServices;

namespace TicketSalesSystem.Service.Images
{
    public class FileService : IFileService
    {
        //基本路徑
        private readonly string _basePath= Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photos");

        //多載A:指定檔案名稱與資料夾(EX固定PID)
        public async Task<string> SaveFileAsync(IFormFile file, string fileName, string folderName)
        {
            return await ProcessUpload(file, folderName, fileName);
        }

        //多載B:自動產生檔案名稱(EX使用GUID)
        public async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();

            var fileName = $"{Guid.NewGuid()}{extension}";

            return await ProcessUpload(file, folderName, fileName);
        }




        private async Task<string> ProcessUpload(IFormFile file, string folderName, string fileName)
        {
            if(file==null|| file.Length==0)
            {                 
                return "";
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            var uploadPath = Path.Combine(_basePath, folderName);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // 🚩 修正 1：如果傳進來的 fileName 已經包含副檔名，就不要再加一次
            // 或是統一外面只傳 ID，這裡補副檔名
            string finalFileName = fileName.EndsWith(extension) ? fileName : $"{fileName}{extension}";
            var filePath = Path.Combine(uploadPath, finalFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            //回傳網址路徑 (供前端 <img> 使用)
            return finalFileName;

        }

        public async Task DeleteFileAsync(string fileName, string folderName)
        {
            var filePath = Path.Combine(_basePath, folderName, fileName);
            if (File.Exists(filePath))
            {
                
                await Task.Run(() => File.Delete(filePath));
            }
        }
    }
}
