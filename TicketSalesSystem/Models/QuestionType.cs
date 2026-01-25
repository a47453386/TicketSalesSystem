using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class QuestionType
    {
        [Key]
        [Column(TypeName = "nchar(1)")]
        public string QuestionTypeID { get; set; } = null!;

        [Display(Name = "問題種類名稱")]
        [Required(ErrorMessage = "必填")]
        [StringLength(10,ErrorMessage = "最多10個字")]
        public string QuestionTypeName { get; set; } = null!;




        //關聯
        public List<Question>? Question { get; set; }
    }
}
