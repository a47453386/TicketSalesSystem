using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class OrderStatus
    {

        [Key]
        [Column(TypeName = "nchar(1)")]
        public string OrderStatusID { get; set; } = null!;

        [Display(Name = "訂單狀態")]
        [Required(ErrorMessage = "必填")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "訂單狀態2~10個字")]
        public string OrderStatusName { get; set; } = null!;

        



        //關聯
        public virtual List<Order>? Order { get; set; }
    }
}
