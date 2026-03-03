namespace TicketSalesSystem.ViewModel.Programme.Index
{
    public class ProgrammeIndexSession
    {
        public string SessionID { get; set; } = null!;
        public DateTime? StartTime { get; set; }
        public DateTime? SaleStartTime { get; set; }
        public DateTime? SaleEndTime { get; set; }
    }
}
