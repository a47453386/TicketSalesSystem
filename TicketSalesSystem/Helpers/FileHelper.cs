namespace TicketSalesSystem.Helpers
{
    public class FileHelper
    {
        // 輔助方法：檢查檔案是否為圖片格式
        public static bool IsImage(string extension)
        {
            // 定義常見的圖片副檔名清單（轉小寫比較，避免大小寫不一致）
            string[] imgExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg" };

            return imgExtensions.Contains(extension.ToLower());
        }
    }
}
