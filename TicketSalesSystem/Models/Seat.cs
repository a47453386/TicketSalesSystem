using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class Seat
    {
        [Key]
        [Column(TypeName ="nchar(2)")]
        public string SeatID { get; set; } = null!;

        [Display(Name = "號")]
        [Required(ErrorMessage = "必填")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "請輸入2個字")]
        [RegularExpression("[0-9]{2}", ErrorMessage = "請輸入2位數字")]
        public string SeatName { get; set; } = null!;

        //FK
        public string SeatStatusID { get; set; } = null!;

        //關聯
        public virtual SeatStatus? SeatStatus { get; set; }

        public virtual List<Tickets>? Tickets { get; set; }
    }
}
