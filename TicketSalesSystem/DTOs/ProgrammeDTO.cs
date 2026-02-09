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
        public string? CoverImage { get; set; }
        public string? SeatImage { get; set; }
        public int? LimitPerOrder { get; set; }
        public DateTime OnShelfTime { get; set; }
        public string? PlaceID { get; set; }

        public string? ProgrammeStatusID { get; set; }
        public string VenueID { get; set; }= null!;
        public string TicketsAreaStatusID { get; set; } = null!;

        public List<DescriptionImageDTO> DescriptionImage { get; set; } = new List<DescriptionImageDTO>();
        public List<SessionDTO>? Session { get; set; } = new List<SessionDTO>();
        public List<TicketsAreaDTO> TicketsArea { get; set; } = new List<TicketsAreaDTO>();

    }
}
