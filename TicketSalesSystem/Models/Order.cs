using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace TicketSalesSystem.Models
{
    public class Order
    {
        [Key]
        [StringLength(12,MinimumLength =12)]
        public string OrderID { get; set; } = null!;

        [Display(Name = "訂單建立時間")]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日 hh:mm:ss}")]
        public DateTime OrderCreatedTime { get; set; }

        //[Display(Name = "訂單保留時間")]
        //[Required(ErrorMessage = "必填")]
        //[DataType(DataType.DateTime)]
        //[DisplayFormat(DataFormatString = "{0:HH:mm}")]
        //public DateTime ExpireAt { get; set; }

        [Display(Name = "付款完成時間")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日 hh:mm:ss}")]
        public DateTime? PaidTime { get; set; }



        //FK區
        public  string MemberID { get; set; } = null!;       
        public  string PaymentMethodID { get; set; } = null!;        
        public string OrderStatusID { get; set; } = null!;     
        public string SessionID { get; set; } = null!;
        public string PaymentTradeNO { get; set; } = null!;




        //關聯區

        public virtual Member? Member { get; set; }
        public virtual PaymentMethod? PaymentMethod { get; set; }
        public virtual OrderStatus? OrderStatus { get; set; }
        public virtual Session? Session { get; set; }

        public virtual Payment? Payment { get; set; }//金流
        public virtual List<Tickets>? Tickets { get; set; }
        
    }
}