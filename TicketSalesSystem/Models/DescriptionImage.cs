using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace TicketSalesSystem.Models
{
    public class DescriptionImage
    {

        [StringLength(36, MinimumLength = 36)]
        public string DescriptionImageID { get; set; } = null!;

        [Display(Name = "說明圖片")]
        [Required(ErrorMessage = "必填")]
        [StringLength(200)]
        public string DescriptionImageName { get; set; } = null!;


        [DataType(DataType.MultilineText)]
        public string ImagePath { get; set; }="";
        //FK            
        public string ProgrammeID { get; set; } = null!;

        //關聯
        public virtual Programme? Programme { get; set; }
    }
}
