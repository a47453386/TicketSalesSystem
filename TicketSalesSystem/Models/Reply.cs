using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class Reply
    {
        [Key]
        [StringLength(36, MinimumLength = 36)]
        public string ReplyID { get; set; } = null!;

        [Display(Name = "回覆內容")]
        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "必填")]
        public string ReplyDescription { get; set; } = null!;

        [Display(Name = "回覆時間")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日 hh:mm:ss}")]
        public DateTime CreatedTime { get; set; } = DateTime.Now;


        [Display(Name = "備註")]
        [DataType(DataType.MultilineText)]
        public string? Note { get; set; }




        //FK區
        public string EmployeeID { get; set; } = null!;
        public string QuestionID { get; set; } = null!;
        public string ReplyStatusID { get; set; } = null!;



        //關聯區
        public virtual Employee? Employee { get; set; }
        public virtual Question? Question { get; set; }
        public virtual ReplyStatus? ReplyStatus { get; set; }

    }
}
