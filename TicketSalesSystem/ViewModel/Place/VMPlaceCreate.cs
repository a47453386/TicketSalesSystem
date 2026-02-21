using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.ViewModel.Place
{
    public class VMPlaceCreate
    {

        [Key]
       
        public string? PlaceID { get; set; } = null!;

        [Display(Name = "場地名稱")]
        [Required(ErrorMessage = "場地名稱必填")]
        [StringLength(20, ErrorMessage = "場地名稱最多20個字")]
        public string PlaceName { get; set; } = null!;

        [Display(Name = "場地地址")]
        [Required(ErrorMessage = "場地地址必填")]
        [StringLength(50, ErrorMessage = "場地地址最多50個字")]
        public string PlaceAddress { get; set; }

        [Display(Name = "場內平面圖")]
        [StringLength(5, MinimumLength = 5)]
        public IFormFile? VenueImage { get; set; }

        public List<VMVenueItem> VenueItem { get; set; } = new List<VMVenueItem>();



        public string? VenueImages { get; set; }


    }
}
