namespace TicketSalesSystem.DTOs.PutDTO
{
    public class TicketsAreaPutDTO
    {
        public string? TicketsAreaID { get; set; } 
        public string VenueID { get; set; }
        public string TicketsAreaName { get; set; }
        public decimal Price { get; set; }
        public int RowCount { get; set; }
        public int SeatCount { get; set; }
        public int Capacity { get; set; }

        public int Remaining { get; set; }

        public string TicketsAreaStatusID { get; set; } = "A";
        
    }
}
