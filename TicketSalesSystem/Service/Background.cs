using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using static System.Formats.Asn1.AsnWriter;
namespace TicketSalesSystem.Service
{
    public class Background : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Background> _logger;

        public Background(IServiceProvider serviceProvider, ILogger<Background> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("正在更新活動狀態，以及檢查過期佔用票券");

            //定期執行與循環
            while (!stoppingToken.IsCancellationRequested)
            {

                try
                {
                    using (var scope = _serviceProvider.CreateScope())//不能直接在背景服務裡注入 DbContext，必須手動建立一個 scope 來取得資料庫連線，確保每次檢查完都會正確關閉連線
                    {
                        //從 DI 容器拿服務，拿不到就直接丟例外
                        var context = scope.ServiceProvider.GetRequiredService<TicketsContext>();
                        //先處理過期票券並歸還庫存
                        await CleanupExpiredTicketsAsync(context);

                        //更新活動售票狀態
                        await UpdateProgrammeStatusAsync(context);

                    }
                }
                catch (Exception ex)
                {
                _logger.LogError(ex, "An error occurred while cleaning up tickets.");
                }
                // 每分鐘跑一次
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            _logger.LogInformation("Ticket Cleanup Service is stopping.");
        }


        //票券釋出
        private async Task CleanupExpiredTicketsAsync(TicketsContext context)
        {
            _logger.LogInformation("正在檢查過期佔用票券...");
            var expirationTime = DateTime.Now.AddMinutes(-10);

            // 我們需要知道哪些區域要補回多少庫存， 找出狀態為 'P' (Pending) 且超時的票券，按區域分組統計
            var expiredGroups = await context.Tickets
                .Where(t => t.TicketsStatusID == "P" && t.CreatedTime <= expirationTime)
                .GroupBy(t => t.TicketsAreaID)
                .Select(g => new { AreaID = g.Key, Count = g.Count() })
                .ToListAsync();

            if (expiredGroups.Any())
            {
                // 使用 Transaction 確保庫存歸還與狀態更新一致
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    foreach (var group in expiredGroups)
                    {
                        //原子性歸還庫存：Remaining = Remaining + N
                        await context.Database.ExecuteSqlInterpolatedAsync(
                            $"UPDATE TicketsArea SET Remaining = Remaining + {group.Count} WHERE TicketsAreaID = {group.AreaID}"
                        );
                    }

                    // 2. 將票券設為失效 'N'(可售)
                    var affectedTickets = await context.Tickets
                        .Where(t => t.TicketsStatusID == "P" && t.CreatedTime <= expirationTime)
                        .ExecuteUpdateAsync(s => s.SetProperty(t => t.TicketsStatusID, "N"));

                    // 3. 將訂單設為失效 'N' (未付款且過期)
                    var affectedOrders = await context.Order
                        .Where(o => o.OrderStatusID != "N" && o.PaymentStatus == false && o.OrderCreatedTime <= expirationTime)
                        .ExecuteUpdateAsync(s => s.SetProperty(o => o.OrderStatusID, "N"));

                    await transaction.CommitAsync();
                    _logger.LogInformation($"[庫存回收] 成功歸還 {expiredGroups.Sum(g => g.Count)} 個座位，取消 {affectedOrders} 筆過期訂單。");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "回收過期票券庫存時失敗");
                }
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
    
