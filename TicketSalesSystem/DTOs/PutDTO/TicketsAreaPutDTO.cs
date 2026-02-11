namespace TicketSalesSystem.DTOs.PutDTO
{
    public class TicketsAreaPutDTO
    {
        public string TicketsAreaID { get; set; } 
        public string VenueID { get; set; }
        public string TicketsAreaName { get; set; }
        public decimal Price { get; set; }
        public int RowCount { get; set; }
        public int SeatCount { get; set; }
        public string TicketsAreaStatusID { get; set; }  
        public string SessionID { get; set; }
    }
}
