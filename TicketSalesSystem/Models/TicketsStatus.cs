using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class TicketsStatus
    {
        [Key]
        [Column(TypeName = "nchar(1)")]
        public string TicketsStatusID { get; set; } = null!;

        [Display(Name = "票券狀態")]
        [Required(ErrorMessage = "必填")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "請輸入2~10個字")]
        public string TicketsStatusName { get; set; } = null!;

        //關聯
        public virtual List<Tickets>? Tickets { get; set; }
    }
}
