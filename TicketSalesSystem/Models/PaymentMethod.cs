using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class PaymentMethod
    {
        [Key]
        [Column(TypeName = "nchar(1)")]
        public string PaymentMethodID { get; set; } = null!;

        [Display(Name = "付款方式")]
        [Required(ErrorMessage = "必填")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "付款方式2~20個字")]
        public string PaymentMethodName { get; set; } = null!;


        //關聯
        public virtual List<Order>? Order { get; set; }

    }
}
