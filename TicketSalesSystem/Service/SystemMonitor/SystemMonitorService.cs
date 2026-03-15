using System.Collections.Concurrent;

namespace TicketSalesSystem.Service.SystemMonitor
{
    public class SystemMonitorService
    {
        // 使用 ConcurrentQueue 確保多執行緒寫入時不會當掉
        private readonly ConcurrentQueue<string> _logs = new();
        private readonly int _maxLogs = 30; // 只保留最近 30 筆，避免記憶體爆掉                                           
        public string CurrentStatus { get; set; } = "待機中"; // 🚩 新增：紀錄當前系統狀態 (例如：掃描中、待機中、回收中)


        // 新增日誌：同時寫入記憶體(前端顯示)與硬碟(永久存檔)
        public void AddLog(string message, string status = "處理中")
        {
            CurrentStatus = status;
            var timeStamp = DateTime.Now.ToString("HH:mm:ss");
            var logEntry = $"[{timeStamp}] {message}";

            // 1. 寫入記憶體 (供網頁端 GetLogs 呼叫)
            _logs.Enqueue(logEntry);

            // 如果超過數量，就把最舊的丟掉 (保持記憶體精簡)
            while (_logs.Count > _maxLogs)
            {
                _logs.TryDequeue(out _);
            }

            // 2. 🚩 寫入實體檔案 (確保重啟後還能看到今天的完整紀錄)
            WriteLogToFile(message, status);
        }

        // 取得所有日誌 (新的在上面)
        public List<string> GetLogs() => _logs.Reverse().ToList();

        // 將日誌寫入到 bin/Debug/net8.0/Logs/ 資料夾下

        private void WriteLogToFile(string message, string status)
        {
            try
            {
                // 取得專案執行目錄下的 Logs 資料夾
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

                // 確保資料夾存在
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                // 以日期為檔名，每天產出一個檔案
                string fileName = $"system-log-{DateTime.Now:yyyy-MM-dd}.txt";
                string filePath = Path.Combine(logDir, fileName);

                // 組合寫入內容
                string fullContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{status}] {message}{Environment.NewLine}";

                // 使用 AppendAllText：如果檔案不存在會建立，存在則接在後方
                File.AppendAllText(filePath, fullContent);
            }
            catch (Exception)
            {
                // 日誌寫入失敗通常不拋出異常，避免中斷主程式運作
            }
        }



    }


}
