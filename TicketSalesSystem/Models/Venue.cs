using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class Venue
    {
        [Key]
        [Column(TypeName = "nchar(3)")]
        public string VenueID { get; set; } = null!;
        
        [Display(Name ="區域名稱")]
        [Required(ErrorMessage ="必填")]
        [StringLength(20,MinimumLength =2,ErrorMessage =("區域名稱2~20個字"))]
        public string VenueName { get; set; } = null!;

        [Display(Name = "樓層")]
        [Required(ErrorMessage = "必填")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = ("區域名稱2~20個字"))]
        public string FloorName { get; set; } = null!;

        [Display(Name = "區域顏色")]
        [Required(ErrorMessage = "必填")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = ("區域名稱2~20個字"))]
        public string AreaColor { get; set; } = null!;

        [Display(Name ="每區總排數")]
        [Required(ErrorMessage ="必填")]
        [Range(0,50,ErrorMessage =("排數為0~50之間"))]
        public int RowCount { get; set; }

        [Display(Name = "每排總座位數")]
        [Required(ErrorMessage = "必填")]
        [Range(0, 50, ErrorMessage = ("排數為0~50之間"))]
        public int SeatCount { get; set; }

        



        //FK
        [Display(Name ="區域狀態")]
        public string VenueStatusID { get; set; } = null!;

        [Display(Name = "場館")]
        public string PlaceID { get; set; } = null!;



        //關聯
        public virtual VenueStatus? VenueStatus { get; set; }

        public virtual Place? Place { get; set; }

        public virtual List<TicketsArea>? TicketsArea { get; set; }


        //計算區
        [Display(Name = "該區座位數")]
        public int TotalSeats => RowCount * SeatCount;
    }
}
