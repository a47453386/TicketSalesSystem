using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class VenueStatus
    {
        [Key]
        [Column(TypeName = "nchar(1)")]
        public string VenueStatusID { get; set; } = null!;

        [Display(Name = "區域狀態名稱")]
        [Required(ErrorMessage = "必填")]
        [StringLength(30, ErrorMessage = "區域狀態名稱最多30個字")]
        public string VenueStatusName { get; set; } = null!;


        //關聯
        public virtual List<Venue>? Venue { get; set; }
    }
}
