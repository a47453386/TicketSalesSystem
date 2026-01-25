using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace TicketSalesSystem.Models
{

    public class Place
    {
        [Key]
        [Column(TypeName = "nchar(1)")]
        public string PlaceID { get; set; } = null!;

        [Display(Name = "場地名稱")]
        [Required(ErrorMessage = "必填")]
        [StringLength(20,ErrorMessage ="場地名稱最多20個字")]
        public string PlaceName { get; set; } = null!;

        [Display(Name = "場地地址")]
        [Required(ErrorMessage = "必填")]
        [StringLength(50, ErrorMessage = "場地地址最多50個字")]
        public string PlaceAddress { get; set; } = null!;


        //關聯
        public virtual List<Programme>? Programme { get; set; } 
    }

}
