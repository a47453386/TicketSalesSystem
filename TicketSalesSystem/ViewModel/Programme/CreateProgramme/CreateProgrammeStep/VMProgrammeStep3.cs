using System.ComponentModel.DataAnnotations;
using TicketSalesSystem.ViewModel.Programme.CreateProgramme.Item;

namespace TicketSalesSystem.ViewModel.Programme.CreateProgramme.CreateProgrammeStep
{
    public class VMProgrammeStep3
    {
        [Key]
        public List<VMTicketsAreaItem> TicketsArea { get; set; } = new List<VMTicketsAreaItem>();

        public List<VMSessionItemForGrammeIndex> Session { get; set; } = new List<VMSessionItemForGrammeIndex>();
        public string? VenueID { get; set; }
        public string TicketsAreaStatusID { get; set; } = "A";
    }
}
