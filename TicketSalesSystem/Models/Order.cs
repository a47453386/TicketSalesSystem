using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace TicketSalesSystem.Models
{
    public class Order
    {
        [Key]
        [StringLength(14,MinimumLength =14)]
        public string OrderID { get; set; } = null!;

        [Display(Name = "訂單建立時間")]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日 hh:mm:ss}")]
        public DateTime OrderCreatedTime { get; set; }= DateTime.Now;
              

        [StringLength(36, MinimumLength = 36)]
        public string? PaymentTradeNO { get; set; }//採用GUID     

        [Display(Name = "金流內容")]
        [StringLength(100, ErrorMessage = "最多100個字")]
        public string? PaymentDescription { get; set; }

        [Display(Name = "金流狀態")]             
        public bool PaymentStatus { get; set; }= false;//false未付款 true已付款

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


        
        




        //關聯區

        public virtual Member? Member { get; set; }
        public virtual PaymentMethod? PaymentMethod { get; set; }
        public virtual OrderStatus? OrderStatus { get; set; }
        public virtual Session? Session { get; set; }

       
        public virtual List<Tickets>? Tickets { get; set; }

        public virtual List<Question>? Question { get; set; }
    }
}