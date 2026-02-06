using System.ComponentModel.DataAnnotations;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.DTOs
{
    public class SessionDTO
    {
        public string SessionID { get; set; } = null!;
        public DateTime SaleStartTime { get; set; }
        public DateTime SaleEndTime { get; set; }
        public DateTime StartTime { get; set; }

        public string ProgrammeID { get; set; } = null!;

        public virtual List<TicketsArea>? TicketsArea { get; set; }

       
    }
}
