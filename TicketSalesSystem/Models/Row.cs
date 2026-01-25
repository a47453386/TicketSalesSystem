using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class Row
    {
        [Key]
        [Column(TypeName = "(nchar(2))")]
        public string RowID { get; set; } = null!;

        [Required(ErrorMessage = "必填")]
        [RegularExpression("[0-9]{2}", ErrorMessage = "請輸入2位數字")]
        public string RowName { get; set; } = null!;
    }
}
