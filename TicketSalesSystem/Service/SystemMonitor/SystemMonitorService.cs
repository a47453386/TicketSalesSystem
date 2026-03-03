using System.Collections.Concurrent;

namespace TicketSalesSystem.Service.SystemMonitor
{
    public class SystemMonitorService
    {
        // 使用 ConcurrentQueue 確保多執行緒寫入時不會當掉
        private readonly ConcurrentQueue<string> _logs = new();
        private readonly int _maxLogs = 30; // 只保留最近 30 筆，避免記憶體爆掉                                           
        public string CurrentStatus { get; set; } = "待機中"; // 🚩 新增：紀錄當前系統狀態 (例如：掃描中、待機中、回收中)

        public void AddLog(string message, string status = "處理中")
        {
            CurrentStatus = status; // 🚩 每次寫 Log 順便更新狀態

            var logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            _logs.Enqueue(logEntry);

            // 如果超過數量，就把最舊的丟掉
            while (_logs.Count > _maxLogs)
            {
                _logs.TryDequeue(out _);
            }
        }

        public List<string> GetLogs() => _logs.Reverse().ToList(); // 新的在上面
    }
}
