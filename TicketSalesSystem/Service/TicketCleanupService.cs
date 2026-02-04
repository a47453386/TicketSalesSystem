using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
namespace TicketSalesSystem.Service
{
    public class TicketCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TicketCleanupService> _logger;

        public TicketCleanupService(IServiceProvider serviceProvider, ILogger<TicketCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //定期執行與循環
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("正在檢查過期佔用票券");
                try
                {
                    using (var scope = _serviceProvider.CreateScope())//不能直接在背景服務裡注入 DbContext，必須手動建立一個 scope 來取得資料庫連線，確保每次檢查完都會正確關閉連線
                    {
                        //取得context資料
                        var Context = scope.ServiceProvider.GetRequiredService<TicketsContext>();
                        //現在時間往前推10分鐘
                        var expirationTime = DateTime.Now.AddMinutes(-10);
                        //取得票券狀態
                        var expiredTickets = Context.Tickets
                            .Where(t => t.TicketsStatusID == "P" && t.CreatedTime <= expirationTime)
                            .ToList();
                        //當有找到任何票券
                        if (expiredTickets.Any())
                        {
                            //Context.Tickets.RemoveRange(expiredTickets);
                            //所有訂單編號
                            var AllOrderID= expiredTickets.Select(t => t.OrderID).Where(id => id != null).Distinct().ToList();

                            //更新票券狀態:釋放座位
                            foreach(var ticket in expiredTickets)
                            {
                                ticket.TicketsStatusID = "N";//改為可售                                
                            }

                            //更新訂單狀態
                            //取得所有未付款訂單
                            var orderUpdtae = await Context.Order.Where(o => AllOrderID.Contains(o.OrderID) && o.PaymentStatus == false).ToListAsync();

                            //更新訂單狀態:付款逾期失效
                            foreach (var order in orderUpdtae)
                            {
                                order.OrderStatusID = "N"; //逾期未付款                               
                            }

                            await Context.SaveChangesAsync();

                            _logger.LogInformation($"[背景服務] 已釋放 {expiredTickets.Count} 個座位，並失效訂單: {string.Join(", ", AllOrderID)}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while cleaning up tickets.");
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            _logger.LogInformation("Ticket Cleanup Service is stopping.");
        }
    }
}
