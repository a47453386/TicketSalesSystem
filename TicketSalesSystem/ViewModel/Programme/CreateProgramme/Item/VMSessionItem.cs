using System.ComponentModel.DataAnnotations;

namespace TicketSalesSystem.ViewModel.Programme.CreateProgramme.Item
{
    public class VMSessionItemForGrammeIndex
    {
        public string? SessionID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime SaleStartTime { get; set; }
        public DateTime SaleEndTime { get; set; }      

        // 可以在這裡加一個 TempId，方便前端刪除特定場次時使用
        public string TempId { get; set; } = Guid.NewGuid().ToString();

        public string? VenueID { get; set; }

        public List<VMTicketsAreaItem> TicketsArea { get; set; } = new List<VMTicketsAreaItem>();
    }
}
