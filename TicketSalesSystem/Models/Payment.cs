using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class Payment
    {

        [Key]
        [StringLength(36, MinimumLength = 36)]
        public string PaymentTradeNO { get; set; } = null!;//採用GUID       


        [Display(Name = "金流內容")]
        [StringLength(100, ErrorMessage = "最多100個字")]        
        public string? PaymentDescription { get; set; } = null!;




        //FK區
        public string OrderID { get; set; } = null!;
        public string PaymentStatusID { get; set; } = null!;

        //關聯區
        public virtual Order? Order { get; set; }
        public virtual PaymentStatus? PaymentStatus { get; set; }
    }
}
