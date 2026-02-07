using System.ComponentModel.DataAnnotations;

namespace TicketSalesSystem.DTOs
{
    public class DescriptionImageDTO
    {
        public string DescriptionImageID { get; set; } = null!;       
        public string DescriptionImageName { get; set; } = null!;

        public string TempUrl { get; set; } = null!;
    }
}
