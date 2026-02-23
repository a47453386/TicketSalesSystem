using Microsoft.Extensions.Caching.Memory;

namespace TicketSalesSystem.Service.Queue
{
    public class QueueService : IQueueService
    {
        private readonly IMemoryCache _memoryCache;
        private const string CacheKey = "ActiveUserCount";

        public QueueService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void ReleaseQueueSlot()
        {
            if (_memoryCache.TryGetValue(CacheKey, out int current))
            {
                if (current > 0)
                {
                    _memoryCache.Set(CacheKey, current - 1, TimeSpan.FromMinutes(10));
                }
            }
        }

        public int GetActiveUserCount()
        {
            return _memoryCache.Get<int>(CacheKey);
        }
    }
}
