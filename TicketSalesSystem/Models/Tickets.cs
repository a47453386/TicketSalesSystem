using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class Tickets
    {
        [Key]
        [Display(Name = "票券號碼")]
        [StringLength(36, MinimumLength = 36)]
        public string TicketsID { get; set; } = null!;

        [Display(Name = "排")]
        public int RowIndex { get; set; }
        [Display(Name = "號")]
        public int SeatIndex { get; set; }

        [Display(Name = "退票處理時間")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd  hh:mm:ss}")]
        public DateTime? RefundTime { get; set; }//退票處理時間

        [Display(Name = "建立時間")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd  hh:mm:ss}")]
        public DateTime CreatedTime { get; set; }= DateTime.Now;

        [Display(Name = "核銷進場時間")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? ScannedTime { get; set; }//核銷進場時間

        [Display(Name = "核銷碼")]
        [StringLength(12)]
        public string? CheckInCode { get; set; }//核銷碼


        //FK             

        public string TicketsStatusID { get; set; } = null!;   
        public string OrderID { get; set; } = null!;
        public string SessionID { get; set; } = null!;
        public string TicketsAreaID { get; set; } = null!;

        //關聯區       
        public virtual TicketsStatus? TicketsStatus { get; set; }       
        public virtual Order? Order { get; set; }
        public virtual Session? Session { get; set; }
        public virtual TicketsArea? TicketsArea { get; set; }


    }
}
