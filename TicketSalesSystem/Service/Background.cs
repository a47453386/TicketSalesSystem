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

        private bool _isFirstRun = true;
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

            while (!stoppingToken.IsCancellationRequested)
            {
                _monitor.AddLog("🔍 啟動定時巡檢...", "掃描中...");

                try
                {
                    // --- 🚩 每日維護任務：每天只跑一次 ---
                    if (DateTime.Now.Date > _lastCleanupDate.Date || _isFirstRun)
                    {
                        _monitor.AddLog("🧹 偵測到跨日，執行日誌清理、解鎖票券與【庫存全量重整】...", "每日維護");

                        await CleanupOldLogsAsync();

                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<TicketsContext>();

                            // 1. 🚩 執行你的 SQL 預存程序 (解鎖 15 天內的票)
                            _monitor.AddLog("🔓 正在執行 15 天內票券解鎖程序...");
                            var result = await context.Database.SqlQueryRaw<int>("EXEC [dbo].[sp_UnlockTicketsBySchedule]").ToListAsync();

                            int updatedCount = result.FirstOrDefault();

                            if (updatedCount > 0)
                            {
                                _monitor.AddLog($"🔓 票券解鎖完成，本次共啟用 {updatedCount} 張票券。", "每日維護");
                            }
                            else
                            {
                                _monitor.AddLog("🔓 執行票券解鎖，今日無須啟用的票券。", "每日維護");
                            }

                            // 2. 執行原有的庫存重整
                            _monitor.AddLog("📊 正在重新計算全量庫存...");
                            await context.Database.ExecuteSqlRawAsync("EXEC [dbo].[USP_RebuildInventory]");

                            // 3. 日誌清理
                            await CleanupOldLogsAsync();
                        }

                        _lastCleanupDate = DateTime.Now.Date;
                        await Task.Delay(1000);
                    }

                    // --- 🚩 每分鐘任務：處理即時邏輯 ---
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<TicketsContext>();

                        _monitor.AddLog("🔍 正在檢查全域票券與活動狀態...", "掃描中");

                        // 1. 處理過期票券 (改狀態為 N，這會自動觸發 SQL Trigger 回補庫存)
                        await CleanupExpiredTicketsAsync(context);

                        // 2. 更新活動售票狀態 (R -> O -> E)
                        await UpdateProgrammeStatusAsync(context);

                        _monitor.AddLog("✅ 巡檢週期完成，未發現異常。", "待機中");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "執行背景任務時發生錯誤。");
                    _monitor.AddLog($"❌ 異常發生: {ex.Message}", "連線異常");
                }

                // 每分鐘巡檢一次
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
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

            var activeProgrammes = await context.Programme
                .Include(p => p.Session)
                    .ThenInclude(s => s.TicketsArea) // 優化：預先抓取區域庫存資料
                .Where(p => p.ProgrammeStatusID == "R" || // 設定完成
                    p.ProgrammeStatusID == "H" || // 已上架
                    p.ProgrammeStatusID == "O" || // 開賣中
                    p.ProgrammeStatusID == "S")   // 已完售
                .ToListAsync();

            bool hasChanged = false;

            foreach (var p in activeProgrammes)
            {
                string oldStatus = p.ProgrammeStatusID;
                string newStatus = oldStatus;

                // 1. 檢查是否結束
                if (p.Session.Any() && p.Session.All(s => s.SaleEndTime <= now))
                {
                    newStatus = "E";
                }

                //2.檢查是否開賣
                if (oldStatus == "R" && p.OnShelfTime<=now)
                {
                    newStatus = "H";
                    _logger.LogInformation($"[自動上架] {p.ProgrammeName} 已達上架時間，狀態切換為 H");
                }

                // 2. 檢查是否開賣
                if ((newStatus == "H" || oldStatus == "H") &&
                p.Session.Any(s => s.SaleStartTime <= now && s.SaleEndTime > now))
                {
                    newStatus = "O";
                    _logger.LogInformation($"[自動開賣] {p.ProgrammeName} 已達售票時間，狀態切換為 O");
                }

                // 3. 處理 O 或 S 的庫存連動切換
                if (newStatus != "E" && (newStatus == "O" || newStatus == "S"))
                {
                    var totalRemaining = p.Session.Sum(s => s.TicketsArea.Sum(a => a.Remaining));
                    newStatus = (totalRemaining <= 0) ? "S" : "O";
                }

                if (newStatus != oldStatus)
                {
                    p.ProgrammeStatusID = newStatus;
                    hasChanged = true;
                    _logger.LogInformation($"[狀態同步] {p.ProgrammeName}: {oldStatus} -> {newStatus}");
                    _monitor.AddLog($"活動 [{p.ProgrammeName}] {oldStatus} > {newStatus}", "自動切換");
                }
            }

            if (hasChanged) await context.SaveChangesAsync();
        }
    }
}
    
