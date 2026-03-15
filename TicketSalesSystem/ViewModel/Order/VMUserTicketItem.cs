namespace TicketSalesSystem.ViewModel.Order
{
    public class VMUserTicketItem
    {
        public string ProgrammeName { get; set; }
        public string OrderID { get; set; }
        public string StartTime { get; set; }
        public string PlaceName { get; set; }
        public decimal FinalAmount { get; set; }
        public string TicketsID { get; set; }

        public string TicketsAreaName { get; set; }
        public string Seat { get; set; }
        public string? CheckInCode { get; set; } // 沒到期前會是 null

        public string? TicketsStatusID { get; set; }
    }
}
