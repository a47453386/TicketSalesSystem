
using System.Runtime.InteropServices;
using TicketSalesSystem.Helpers;

namespace TicketSalesSystem.Service.Images
{
    public class FileService : IFileService
    {
        //基本路徑
        private readonly string _basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

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

            // 限制檔案大小，避免過大的檔案造成伺服器負擔
            long maxSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxSize)
            {
                throw new Exception("檔案大小不得超過 5MB");
            }

            // 取得檔案副檔名，並轉為小寫以確保一致性
            var extension = Path.GetExtension(file.FileName).ToLower();

            // 根據副檔名判斷檔案類別，圖片存放在 "Photos" 資料夾，其他文件存放在 "Docs" 資料夾
            string category = FileHelper.IsImage(extension) ? "Photos" : "Docs";

            // 組合完整的上傳路徑，包含基本路徑、類別資料夾和指定的資料夾名稱
            var uploadPath = Path.Combine(_basePath, category, folderName);


            // 確保上傳路徑存在，如果不存在則創建
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // 確保檔案名稱包含正確的副檔名，如果沒有則自動添加
            string finalFileName = fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? fileName : $"{fileName}{extension}";

            // 組合完整的檔案路徑    
            var filePath = Path.Combine(uploadPath, finalFileName);

            // 使用 FileStream 將檔案寫入指定路徑
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 回傳最終的檔案名稱，包含副檔名
            return finalFileName;

        }

        public async Task<bool> DeleteFileAsync(string fileName, string folderName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;

            var filePath = Path.Combine(_basePath, folderName, fileName);

            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    await Task.Run(() => System.IO.File.Delete(filePath));
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
