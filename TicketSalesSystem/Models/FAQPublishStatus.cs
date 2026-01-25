using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class FAQPublishStatus
    {
        [Key]
        [Column(TypeName = "nchar(1)")]
        public string FAQPublishStatusID { get; set; } = null!;

        [Display(Name = "常見問題狀態")]
        [Required(ErrorMessage = "必填")]
        [StringLength(6,ErrorMessage = "狀態最多6個字")]
        public string FAQPublishStatusName { get; set; } = null!;



        //關聯
        public virtual List<FAQ>? FAQ { get; set; }
    }
}
