namespace TicketSalesSystem.DTOs
{
    public class TicketsAreaDTO
    {
        public string TicketsAreaID { get; set; } = null!;
        public string TicketsAreaName { get; set; } = null!;
        public int RowCount { get; set; }
        public int SeatCount { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public int Remaining { get; set; }

        public string TicketsAreaStatusID { get; set; } = "A";
        public string VenueID { get; set; } = null!;
        public string? VenueName { get; set; }
        public string? AreaColor { get; set; } 
        public string SessionID { get; set; } = null!;
    }
}
