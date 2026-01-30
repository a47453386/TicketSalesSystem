using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class TicketsArea
    {
        [Key]
        [Column(TypeName = "nchar(3)")]
        public string TicketsAreaID { get; set; } = null!;

        [Display(Name = "票區")]
        [Required(ErrorMessage = "必填")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "請輸入2~20個字")]
        public string TicketsAreaName { get; set; } = null!;

        [Display(Name = "票價")]
        [Required(ErrorMessage = "必填")]
        [Column(TypeName = "Money")]
        [Range(0, 100000, ErrorMessage = "票價必須在 0 到 100,000 之間")]
        public decimal Price { get; set; } 


        //FK
        public string TicketsAreaStatusID { get; set; } = null!;
        public string VenusID { get; set; } = null!;
        public string ProgrammeID { get; set; } = null!;

        //關聯
        public virtual TicketsAreaStatus? TicketsAreaStatus { get; set; }        
        public virtual Venue? Venus { get; set; }
        public virtual Programme? Programme { get; set; }
    }
}
