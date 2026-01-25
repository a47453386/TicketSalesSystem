using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace TicketSalesSystem.Models
{
    public class Session
    {
        [Key]
        [Column(TypeName = "nchar(2)")]
        public string SessionID { get; set; }= null!;

        [Display(Name = "開賣時間")]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日 hh:mm:ss}")]
        public DateTime SaleStartTime { get; set; }

        [Display(Name = "停售時間")]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日 hh:mm:ss}")]
        public DateTime SaleEndTime { get; set; }

        [Display(Name = "演出日期")]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日 HH:mm}")]
        public DateTime StartTime { get; set; }


        //FK區
        public string ProgrammeID { get; set; } = null!;

        //關聯區
        public virtual Programme? Programme { get; set; }
    }
}
