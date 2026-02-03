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
           
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("正在檢查過期佔用票券");
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var Context = scope.ServiceProvider.GetRequiredService<TicketsContext>();
                        var expirationTime = DateTime.Now.AddMinutes(-10);
                        var expiredTickets = Context.Tickets
                            .Where(t => t.TicketsStatusID == "P" && t.CreatedTime <= expirationTime)
                            .ToList();
                        if (expiredTickets.Any())
                        {
                            Context.Tickets.RemoveRange(expiredTickets);
                            await Context.SaveChangesAsync();
                            _logger.LogInformation($"已成功釋放 {expiredTickets.Count} 張過期票券。");
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
