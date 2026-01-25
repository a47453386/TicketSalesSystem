using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class PaymentStatus
    {
        [Key]
        [Column(TypeName = "nchar(1)")]
        public string PaymentStatusID { get; set; } = null!;



        [Display(Name = "金流狀態")]
        [Required(ErrorMessage = "必填")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "金流狀態2~6個字")]
        public string PaymentStatusName { get; set; } = null!;


        //關聯區
        public virtual List<Payment>? Payment { get; set; }
    }
}
