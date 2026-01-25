using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class TicketsAreaStatus
    {
        [Key]
        [Column(TypeName = "nchar(1)")]
        public string TicketsAreaStatusID { get; set; } = null!;

        [Display(Name = "票區狀態")]
        [Required(ErrorMessage = "必填")]
        [StringLength(2, MinimumLength = 30, ErrorMessage = "請輸入2~30個字")]
        public string TicketsAreaStatusName { get; set; } = null!;

        //關聯
        public virtual List<TicketsArea>? TicketsArea { get; set; }
    }
}
