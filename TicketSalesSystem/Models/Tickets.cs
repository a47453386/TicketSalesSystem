using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class Tickets
    {
        [Key]
        [Display(Name = "票券號碼")]
        [Column(TypeName = "nchar(7)")]        
        public string TicketsID { get; set; } = null!;
   

        [Display(Name = "退票處理時間")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd  hh:mm:ss}")]
        public DateTime? RefundTime { get; set; }//退票處理時間

        [Display(Name = "建立時間")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd  hh:mm:ss}")]
        public DateTime CreatedTime { get; set; }

        [Display(Name = "核銷進場時間")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? ScannedTime { get; set; }//核銷進場時間

       


        //FK             
        
        public string TicketsStatusID { get; set; } = null!;   
        public string OrderID { get; set; } = null!;
        

        //關聯區       
      
        public virtual TicketsStatus? TicketsStatus { get; set; }
        public virtual Order? Order { get; set; }        
     

    }
}
