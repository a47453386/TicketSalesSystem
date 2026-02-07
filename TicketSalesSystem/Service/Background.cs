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
                        await CleanupExpiredTicketsAsync(context);

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

            //現在時間往前推10分鐘
            var expirationTime = DateTime.Now.AddMinutes(-10);
            //取得票券狀態
            var expiredTickets = await context.Tickets
                .Where(t => t.TicketsStatusID == "P" && t.CreatedTime <= expirationTime)
                .ToListAsync();

            //當有找到任何票券
            if (expiredTickets.Any())
            {
                //Context.Tickets.RemoveRange(expiredTickets);
                //所有訂單編號
                var AllOrderID = expiredTickets.Select(t => t.OrderID).Where(id => id != null).Distinct().ToList();

                //更新票券狀態:釋放座位
                foreach (var ticket in expiredTickets)
                {
                    ticket.TicketsStatusID = "N";//改為可售                                
                }

                //更新訂單狀態
                //取得所有未付款訂單
                var orderUpdtae = await context.Order.Where(o => AllOrderID.Contains(o.OrderID) && o.PaymentStatus == false).ToListAsync();

                //更新訂單狀態:付款逾期失效
                foreach (var order in orderUpdtae)
                {
                    order.OrderStatusID = "N"; //逾期未付款                               
                }

                await context.SaveChangesAsync();
                _logger.LogInformation($"[背景服務] 已釋放 {expiredTickets.Count} 個座位，並失效訂單: {string.Join(", ", AllOrderID)}");
            }
        }



        private async Task UpdateProgrammeStatusAsync(TicketsContext context)
        {
            _logger.LogWarning("==== 正在執行活動狀態更新檢查 ====");
            _logger.LogInformation("正在更新活動售票狀態...");
            var now = DateTime.Now;
            _logger.LogInformation($"[檢查狀態] 目前系統時間: {now:yyyy-MM-dd HH:mm:ss}");

            // 一次抓出所有「非結束」且「非手動停用」的活動，減少多次資料庫查詢
            //// R (已設定) 被排除在外，除非管理員手動改為 B，否則排程不理它
            var activeProgrammes = await context.Programme
                .Include(p => p.Session)// 預先載入場次資料，避免後續多次查詢
                .Where(p =>  p.ProgrammeStatusID == "B"|| p.ProgrammeStatusID == "O")                
                .ToListAsync();
            bool hasChanged = false;

            foreach (var p in activeProgrammes)
            {
                var oldStatus = p.ProgrammeStatusID;

                //是否所有場次都結束了?(B 或 O 都有可能轉 E)
                if (p.Session.Any() && p.Session.All(s => s.SaleEndTime <= now))
                {
                    oldStatus = "E";
                    _logger.LogInformation($"[狀態更新] 活動 {p.ProgrammeName}：售票結束 [E: 已結束]");
                    continue;
                }

                // 狀態轉移：只有 B (上架中) 且時間到了，才能轉 O (開賣中)
                if (p.ProgrammeStatusID == "B" && p.Session.Any(s => s.SaleStartTime <= now && s.SaleEndTime > now))
                {
                    oldStatus = "O";
                    _logger.LogInformation($"[狀態更新] {p.ProgrammeName}：開賣中(O)");
                }
                if (oldStatus != p.ProgrammeStatusID)
                {
                    _logger.LogInformation($"[狀態更新] 活動: {p.ProgrammeName} 轉移: {oldStatus} -> {p.ProgrammeStatusID}");
                    hasChanged = true;
                }
                if (hasChanged)
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation("[系統通知] 售票狀態批次更新存檔成功。");
                }

            }

           

        }
    }
}
