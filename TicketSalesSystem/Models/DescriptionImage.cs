using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace TicketSalesSystem.Models
{
    public class DescriptionImage
    {
        
        [Column(TypeName = "nchar(3)")]
        public string DescriptionImageID { get; set; } = null!;

        [Display(Name = "說明圖片")]
        [Required(ErrorMessage = "必填")]
        [StringLength(40, MinimumLength = 40)]
        public string ImageUrl { get; set; } = null!;

        //FK            
        public string ProgrammeID { get; set; } = null!;

        //關聯
        public virtual Programme? Programme { get; set; }
    }
}
