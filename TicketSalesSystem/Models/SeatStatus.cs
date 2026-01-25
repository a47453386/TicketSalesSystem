using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class SeatStatus
    {
        [Key]
        [Column(TypeName = "nchar(2)")]
        public string SeatStatusID { get; set; } = null!;

        [Display(Name = "座位狀態")]
        [Required(ErrorMessage = "必填")]
        [StringLength(2, MinimumLength = 10, ErrorMessage = "請輸入2~10個字")]
        public string SeatStatusName { get; set; } = null!;


        //關聯
        public virtual List<Seat>? Seat { get; set; }
    }
}
