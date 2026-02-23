namespace TicketSalesSystem.Service.Queue
{
    public interface IQueueService
    {
        void ReleaseQueueSlot();
        int GetActiveUserCount();
    }
}
