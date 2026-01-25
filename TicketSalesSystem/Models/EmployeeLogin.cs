using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class EmployeeLogin
    {
        [Key]
        [Display(Name = "帳號")]
        [Required(ErrorMessage = "必填")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "帳號長度需介於 8 到 30 個字元")]
        [RegularExpression("[a-zA-Z0-9]*$", ErrorMessage = "帳號只能包含英文字母、數字")]        
        public string Account { get; set; } = null!;

        [Display(Name = "密碼")]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.Password)]
        [StringLength(200, MinimumLength = 8, ErrorMessage = "密碼長度至少需 8 位")]
        [RegularExpression("(?=.*[a-z])(?=.*[A-Z])(?=.[0-9]){8,}$", ErrorMessage = "密碼必須包含至少一個大寫字母、一個小寫字母與一個數字")]
        
        public string Password { get; set; } = null!;








        //FK區域
        [Key]
        [ForeignKey("Member")]
        public string EmployeeID { get; set; } = null!;

        //關聯
        public virtual Employee? Employee { get; set; }
    }
}
