using System.ComponentModel.DataAnnotations;
using TicketSalesSystem.DTOs;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel.CreateProgramme.Item;

namespace TicketSalesSystem.ViewModel.CreateProgramme.CreateProgrammeStep
{
    public class VMProgrammeStep2
    {
        [Key]
        public List<VMSessionItem> Session { get; set; } = new List<VMSessionItem>();
        
        public string PlaceID { get; set; }
    }
}
