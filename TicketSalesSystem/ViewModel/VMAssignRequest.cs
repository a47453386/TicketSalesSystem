using System.ComponentModel.DataAnnotations;

namespace TicketSalesSystem.ViewModel
{
    public class VMAssignRequest
    {
        [Key]
        public string ProgramID { get; set; }
        public string VenueID { get; set; }
        public string SessionID { get; set; }
        public string AreaID { get; set; }
        public int Count { get; set; }
    }
}
