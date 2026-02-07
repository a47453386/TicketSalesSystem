
using System.ComponentModel.DataAnnotations;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.ViewModel.CreateProgramme.CreateProgrammeStep
{
    public class VMProgrammeStep1
    {
        [Key]
        
        public string ProgrammeName { get; set; } = null!;
        
        public string ProgrammeDescription { get; set; } = null!;
        public int? LimitPerOrder { get; set; }
        
        public DateTime OnShelfTime { get; set; }
       
        public string? PlaceID { get; set; }

        public string? ProgrammeStatusID { get; set; }
    }
}
