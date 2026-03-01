using System.ComponentModel.DataAnnotations;

namespace TicketSalesSystem.DTOs
{
    public class DescriptionImageDTO
    {
        public string? DescriptionImageID { get; set; } 

        public string? DescriptionImageName { get; set; } 

        public IFormFile? ImageFile { get; set; }
        public string? TempUrl { get; set; } 
    }
}
