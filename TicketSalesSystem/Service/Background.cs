using Microsoft.EntityFrameworkCore;
using System.Threading;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.SystemMonitor;
using TicketSalesSystem.ViewModel;
using static System.Formats.Asn1.AsnWriter;
namespace TicketSalesSystem.Service
{
    public class Background : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Background> _logger;
        private DateTime _lastCleanupDate = DateTime.Now.Date;
        private readonly SystemMonitorService _monitor;

        public Background(IServiceProvider serviceProvider, ILogger<Background> logger, SystemMonitorService monitor)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _monitor = monitor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("正在更新活動狀態，以及檢查過期佔用票券");
            _monitor.AddLog("🚀 系統核心已啟動...");

            //定期執行與循環
            while (!stoppingToken.IsCancellationRequested)
            {
                _monitor.AddLog("🔍啟動定時巡檢...", "掃描中...");
                await Task.Delay(2000);
                try
                {
                    if (DateTime.Now.Date > _lastCleanupDate.Date)
                    {
                        _monitor.AddLog("🧹 偵測到跨日，執行舊日誌清理...", "清理日誌");
                        await CleanupOldLogsAsync();
                        _lastCleanupDate = DateTime.Now;
                        await Task.Delay(1000);
                    }

                    using (var scope = _serviceProvider.CreateScope())//不能直接在背景服務裡注入 DbContext，必須手動建立一個 scope 來取得資料庫連線，確保每次檢查完都會正確關閉連線
                    {
                        //從 DI 容器拿服務，拿不到就直接丟例外
                        var context = scope.ServiceProvider.GetRequiredService<TicketsContext>();

                        _monitor.AddLog("🔍 正在檢查全域票券與活動狀態...", "掃描中");
                        await Task.Delay(2000);// 模擬作業時間 (測試用，正式上線可移除)

                        //先處理過期票券並歸還庫存
                        await CleanupExpiredTicketsAsync(context);
                        //更新活動售票狀態
                        await UpdateProgrammeStatusAsync(context);


                        _monitor.AddLog("✅ 巡檢週期完成，未發現異常。", "待機中");
                    }
                   
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "執行背景任務時發生錯誤。");
                    _monitor.AddLog($"❌ 異常發生: {ex.Message}", "連線異常");
                }
               
                // 每分鐘跑一次
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            _logger.LogInformation("Ticket Cleanup Service is stopping.");
            _monitor.AddLog("🛑 系統核心已關閉", "已停止");
        }

        //日誌清理
        private async Task CleanupOldLogsAsync()
        {
            try
            {
                _logger.LogInformation("正在執行舊日誌清理作業...");

                // 設定日誌存放的路徑 (假設你的日誌在專案根目錄的 Logs 資料夾)
                string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

                if (!Directory.Exists(logDirectory))
                {
                    _logger.LogWarning($"日誌目錄不存在：{logDirectory}");
                    return;
                }

                int daysToKeep = 1; // 🚩 設定保留天數，超過 1 天就刪除
                var threshold = DateTime.Now.AddDays(-daysToKeep);

                var directoryInfo = new DirectoryInfo(logDirectory);
                // 搜尋所有日誌檔 (*.txt 或 *.log)
                var files = directoryInfo.GetFiles("*.*")
                    .Where(f => f.LastWriteTime < threshold);

                int count = 0;
                foreach (var file in files)
                {
                    try
                    {
                        file.Delete();
                        count++;
                    }
                    catch (IOException ex)
                    {
                        // 檔案可能正被系統佔用中，跳過不處理
                        _logger.LogWarning($"無法刪除檔案 {file.Name}: {ex.Message}");
                    }
                }

                _logger.LogInformation($"日誌清理完成。共刪除 {count} 個過期檔案。");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "執行日誌清理時發生異常。");
            }

            await Task.CompletedTask;
        }

        //票券釋出
        private async Task CleanupExpiredTicketsAsync(TicketsContext context)
        {

            _logger.LogInformation("正在檢查過期佔用訂單與票券...");
            // 🚩 建議加一點緩衝（例如 11 分鐘），給前端 10 分鐘倒數一點餘裕
            var expirationTime = DateTime.Now.AddMinutes(-11);

            // 1. 找出所有「待付款」且「已過期」的訂單 ID 及其票券資訊
            var expiredOrdersData = await context.Order
                .Where(o => o.OrderStatusID == "P" && o.OrderCreatedTime <= expirationTime && o.PaymentStatus == false)
                .Select(o => new {
                    o.OrderID,
                    Tickets = o.Tickets.Select(t => new { t.TicketsID, t.TicketsAreaID })
                })
                .ToListAsync();

            if (!expiredOrdersData.Any()) return;

            var orderIds = expiredOrdersData.Select(x => x.OrderID).ToList();
            var ticketIds = expiredOrdersData.SelectMany(x => x.Tickets).Select(t => t.TicketsID).ToList();

            // 按區域計算要歸還的庫存
            var areaReturnGroups = expiredOrdersData
                .SelectMany(x => x.Tickets)
                .GroupBy(t => t.TicketsAreaID)
                .Select(g => new { AreaID = g.Key, Count = g.Count() });

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                //C# 負責決策：判定哪些訂單超時了，並發出「處決」指令（將狀態改為 N）。
                //Trigger 負責體力活：只要看到票券狀態變了，就自動去後台把庫存加回去。

                // 1. 只改票券狀態 (這會觸發 Trigger 自動加回庫存)
                await context.Tickets
                    .Where(t => ticketIds.Contains(t.TicketsID))
                    .ExecuteUpdateAsync(s => s.SetProperty(t => t.TicketsStatusID, "N"));

                // 2. 只改訂單狀態
                await context.Order
                    .Where(o => orderIds.Contains(o.OrderID))
                    .ExecuteUpdateAsync(s => s.SetProperty(o => o.OrderStatusID, "N"));

                await transaction.CommitAsync();
                _logger.LogInformation($"[回收成功] 取消訂單: {orderIds.Count} 筆, 釋出票券: {ticketIds.Count} 張。");

                _monitor.AddLog($"成功回收 {orderIds.Count} 筆訂單，釋出 {ticketIds.Count} 張票。", "回收完成");

                
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "執行庫存回收事務時發生異常");
            }

           
        }



        private async Task UpdateProgrammeStatusAsync(TicketsContext context)
        {
            _logger.LogInformation("正在同步活動售票狀態...");
            var now = DateTime.Now;

            // 抓出 R (設定完成) 和 O (開賣中) 的活動進行判斷
            var activeProgrammes = await context.Programme
                .Include(p => p.Session)
                .Where(p => p.ProgrammeStatusID == "R" || p.ProgrammeStatusID == "O")
                .ToListAsync();

            bool hasChanged = false;

            foreach (var p in activeProgrammes)
            {
                string oldStatus = p.ProgrammeStatusID;
                string newStatus = oldStatus;

                // 檢查是否所有場次售票都已結束 -> 轉為 E (Ended)
                if (p.Session.Any() && p.Session.All(s => s.SaleEndTime <= now))
                {
                    newStatus = "E";
                }
                // 檢查是否已達開賣時間 -> R 轉 O (On Sale)
                else if (oldStatus == "R" && p.Session.Any(s => s.SaleStartTime <= now && s.SaleEndTime > now))
                {
                    newStatus = "O";
                }

                // 🚩 修正：必須賦值給實體，SaveChangesAsync 才會生效
                if (newStatus != oldStatus)
                {
                    p.ProgrammeStatusID = newStatus;
                    hasChanged = true;
                    _logger.LogInformation($"[活動狀態切換] {p.ProgrammeName}: {oldStatus} -> {newStatus}");
                }
            }

            if (hasChanged)
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("[系統通知] 活動狀態已批次更新。");
            }
        }
    }
}
    
