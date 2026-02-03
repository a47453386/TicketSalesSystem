
using System.ComponentModel.DataAnnotations;

namespace TicketSalesSystem.ViewModel
{
    public class VMSeats
    {
        [Key]
        public string SeatID { get; set; } = null;
        public int SeatIndex { get; set; }
        public int RowIndex { get; set; }
        public string Label { get; set; } = null;
        public string Status { get; set; } = "可售";    

        
    }
}
