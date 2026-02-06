using Microsoft.Build.Framework;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.ViewModel.CreateProgramme.CreateProgrammeStep
{
    public class VMProgrammeStep1
    {
        [Required]
        public string ProgrammeName { get; set; } = null!;
        [Required]
        public string ProgrammeDescription { get; set; } = null!;
        public int? LimitPerOrder { get; set; }
        [Required]
        public DateTime OnShelfTime { get; set; }
        [Required]
        public string? PlaceID { get; set; }
       
     
    }
}
