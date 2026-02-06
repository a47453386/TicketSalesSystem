using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.DTOs
{
    public class ProgrammeDTO
    {
        public string ProgrammeID { get; set; } = null!;     
        public string ProgrammeName { get; set; } = null!;      
        public string ProgrammeDescription { get; set; } = null!;          
        public IFormFile? CoverImage { get; set; }
        public IFormFile? SeatImage { get; set; }
        public int? LimitPerOrder { get; set; }
        public DateTime OnShelfTime { get; set; }
        public string? PlaceID { get; set; }

        public string? ProgrammeStatusID { get; set; }

        public virtual List<DescriptionImage>? DescriptionImage { get; set; }
        public List<SessionDTO> Sessions { get; set; } = new List<SessionDTO>();
        public List<TicketsAreaDTO> TicketsAreas { get; set; } = new List<TicketsAreaDTO>();

    }
}
