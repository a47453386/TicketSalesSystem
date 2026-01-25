using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class ReplyStatus
    {
        [Key]        
        [Column(TypeName = "nchar(1)")]
        public string ReplyStatusID { get; set; } = null!;

        [Display(Name = "回覆狀態名稱")]
        [Required(ErrorMessage = "必填")]
        [StringLength(10, ErrorMessage = "回復狀態最多10個字")]
        public string ReplyStatusName { get; set; } = null!;







        //關聯
        public virtual List<Reply>? Reply { get; set; }
    }
}
