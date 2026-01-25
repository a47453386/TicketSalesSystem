using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class SeatRow
    {
        [Key]
        [Column(TypeName = "nchar(2)")]
        public string RowID { get; set; } = null!;

        [Display(Name = "排")]
        [Required(ErrorMessage = "必填")]
        [RegularExpression("[0-9]{2}", ErrorMessage = "請輸入2位數字")]
        public string RowName { get; set; } = null!;


        //FK
        public virtual List<Tickets>? Tickets { get; set; }
    }
}
