using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace TicketSalesSystem.Models
{
    public class ProgrammeStatus
    {
        [Key]
        [Column(TypeName = "nchar(1)")]
        public string ProgrammeStatusID { get; set; }= null!;

        [Display(Name = "活動狀態名稱")]
        [Required(ErrorMessage ="必填")]
        [StringLength(10, ErrorMessage = "活動狀態名稱最多10個字")]
        public string ProgrammeStatusName { get; set; }= null!;

        //關聯
        public virtual List<Programme>? Programme { get; set; }
    }
}
