using System.ComponentModel.DataAnnotations;
using TicketSalesSystem.ViewModel.CreateProgramme.Item;

namespace TicketSalesSystem.ViewModel.CreateProgramme.CreateProgrammeStep
{
    public class VMProgrammeStep3
    {
        [Key]
        public List<VMTicketsAreaItem> TicketsArea { get; set; } = new List<VMTicketsAreaItem>();

        public List<VMSessionItem> Session { get; set; } = new List<VMSessionItem>();
        public string VenueID { get; set; } = null!;
        public string TicketsAreaStatusID { get; set; } = "A";
    }
}
