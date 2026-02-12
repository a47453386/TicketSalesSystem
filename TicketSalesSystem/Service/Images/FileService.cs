
using System.Runtime.InteropServices;

namespace TicketSalesSystem.Service.Images
{
    public class FileService : IFileService
    {
        //基本路徑
        private readonly string _basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photos");

        //多載A:指定檔案名稱與資料夾(EX固定PID)
        public async Task<string> SaveFileAsync(IFormFile file, string fileName, string folderName)
        {
            return await ProcessUpload(file, folderName, fileName);
        }

        //多載B:自動產生檔案名稱(EX使用GUID)
        public async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            // 只產生 GUID 字串，副檔名交給底層補齊，保持邏輯一致
            string newGuidName = Guid.NewGuid().ToString();

            return await ProcessUpload(file, folderName, newGuidName);
        }




        private async Task<string> ProcessUpload(IFormFile file, string folderName, string fileName)
        {
            //if (file == null || file.Length == 0||fileName==null)
            //{
            //    return "";
            //}

            var extension = Path.GetExtension(file.FileName).ToLower();
            var uploadPath = Path.Combine(_basePath, folderName);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // 🚩 修正 1：如果傳進來的 fileName 已經包含副檔名，就不要再加一次
            // 或是統一外面只傳 ID，這裡補副檔名
            string finalFileName = fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? fileName : $"{fileName}{extension}";
            var filePath = Path.Combine(uploadPath, finalFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            //回傳(存入資料庫 ImageUrl 欄位)
            return finalFileName;

        }

        public async Task<bool> DeleteFileAsync(string fileName, string folderName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;

            var filePath = Path.Combine(_basePath, folderName, fileName);

            try
            {
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    return true;
                }
                return false;
            }
            catch (IOException e)
            {
                //處理刪除檔案時可能發生的例外狀況，例如檔案正在被使用中或權限不足等
                Console.WriteLine($"檔案 IO 錯誤: {e.Message}");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                // 處理權限不足的異常
                Console.WriteLine($"權限不足，無法刪除檔案: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                // 處理其他非預期錯誤
                Console.WriteLine($"刪除檔案時發生非預期錯誤: {ex.Message}");
                return false;
            }
        }
    }
}
