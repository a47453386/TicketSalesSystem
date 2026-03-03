using System.ComponentModel.DataAnnotations;
using TicketSalesSystem.DTOs;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel.Programme.CreateProgramme.Item;

namespace TicketSalesSystem.ViewModel.Programme.CreateProgramme.CreateProgrammeStep
{
    public class VMProgrammeStep2
    {
        
        public List<VMSessionItemForGrammeIndex> Session { get; set; } = new List<VMSessionItemForGrammeIndex>();
        [Key]
        public string PlaceID { get; set; }
    }
}
