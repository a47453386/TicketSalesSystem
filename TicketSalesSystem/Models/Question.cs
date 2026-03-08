using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class Question
    {
            [Key]
            [StringLength(36, MinimumLength = 36)]
            public string QuestionID { get; set; } = null!;

            [Display(Name = "問題標題")]
            [Required(ErrorMessage = "必填")]
            [StringLength(100, MinimumLength = 2, ErrorMessage = "標題最多100個字")]
            public string QuestionTitle { get; set; } = null!;

            [Display(Name = "問題內容")]
            [Required(ErrorMessage = "必填")]
            [DataType(DataType.MultilineText)]
            public string QuestionDescription { get; set; } = null!;

            [Display(Name = "提問時間")]
            [Required(ErrorMessage = "必填")]
            [DataType(DataType.DateTime)]
            [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日 hh:mm:ss}")]
            public DateTime CreatedTime { get; set; } = DateTime.Now;

            [Display(Name = "上傳檔案")]
            [StringLength(40, MinimumLength = 40)]
            public string? UploadFile { get; set; } = null!;



        //FK區
        public string MemberID { get; set; } = null!;
        public string QuestionTypeID { get; set; } = null!;

        [StringLength(14, MinimumLength = 14)]
        public string? OrderID { get; set; } 


        //關聯區
        public virtual Member? Member { get; set; } 
        public virtual QuestionType? QuestionType { get; set; }

        [ForeignKey(nameof(OrderID))]
        public virtual Order? Order { get; set; }


        public virtual List<Reply>? Reply { get; set; }





    }
}
