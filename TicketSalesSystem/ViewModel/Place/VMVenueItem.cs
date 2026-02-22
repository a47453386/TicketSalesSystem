using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.ViewModel.Place
{
    public class VMVenueItem
    {

        [Column(TypeName = "nchar(3)")]
        public string? VenueID { get; set; }

        [Display(Name = "區域名稱")]        
        [StringLength(20, MinimumLength = 2, ErrorMessage = ("區域名稱2~20個字"))]
        public string? VenueName { get; set; }

        [Display(Name = "樓層")]        
        [StringLength(20, MinimumLength = 2, ErrorMessage = ("區域名稱2~20個字"))]
        public string FloorName { get; set; } 
        [Display(Name = "區域顏色")]
       
        [StringLength(20, MinimumLength = 2, ErrorMessage = ("區域名稱2~20個字"))]
        public string AreaColor { get; set; }

        [Display(Name = "每區總排數")]
       
        [Range(1, 50, ErrorMessage = ("排數為1~50之間"))]
        public int RowCount { get; set; }

        [Display(Name = "每排總座位數")]
        
        [Range(1, 50, ErrorMessage = ("排數為1~50之間"))]
        public int SeatCount { get; set; }
        public string? VenueStatusID { get; set; } 

        [Display(Name = "區域狀態名稱")]
       
        [StringLength(30, ErrorMessage = "區域狀態名稱最多30個字")]
        public string? VenueStatusName { get; set; }
    }
}
