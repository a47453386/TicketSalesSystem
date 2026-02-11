namespace TicketSalesSystem.DTOs.PutDTO
{
    public class TicketsAreaPutDTO
    {
        public string TicketsAreaID { get; set; } // 新增時為空
        public string VenueID { get; set; }
        public string TicketsAreaName { get; set; }
        public decimal Price { get; set; }
        public int RowCount { get; set; }
        public int SeatCount { get; set; }
    }
}
