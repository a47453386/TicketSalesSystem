namespace TicketSalesSystem.DTOs.PutDTO
{
    public class SessionPutDTO
    {
        public string SessionID { get; set; } // 新增時為空或 null
        public DateTime StartTime { get; set; }
        public DateTime SaleStartTime { get; set; }
        public DateTime SaleEndTime { get; set; }
        public List<TicketsAreaPutDTO> TicketsArea { get; set; } = new List<TicketsAreaPutDTO>();
    }
}
