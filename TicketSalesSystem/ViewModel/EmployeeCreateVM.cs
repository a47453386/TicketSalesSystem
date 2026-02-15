using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TicketSalesSystem.Models;
using TicketSalesSystem.ValidationAttributes;

namespace TicketSalesSystem.ViewModel
{
    public class EmployeeCreateVM
    {
        [Key]
        [Display(Name = "員工編號")]
        [Column(TypeName = "nchar(6)")]
        public string EmployeeID { get; set; } 

        [Display(Name = "員工姓名")]
        [Required(ErrorMessage = "必填")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "請輸入2~20個字")]
        public string Name { get; set; } 

        [Display(Name = "到職日期")]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日}")]
        public DateTime HireDate { get; set; }


        [Display(Name = "地址")]
        [Required(ErrorMessage = "必填")]
        [StringLength(50, ErrorMessage = "最多50個字")]
        public string Address { get; set; } 


        [Display(Name = "生日")]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日}")]
        public DateTime Birthday { get; set; }

        [Display(Name = "手機電話")]
        [Required(ErrorMessage = "必填")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "請輸入10碼")]
        [RegularExpression("09[0-9]{8}", ErrorMessage = "手機格式錯誤，請輸入 09 開頭的 10 位數字")]
        public string Tel { get; set; }

        [Display(Name = "性別")]
        [Required(ErrorMessage = "必填")]
        public bool Gender { get; set; }

        [Display(Name = "身分證字號")]
        [Required(ErrorMessage = "必填")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "請輸入10碼")]
        [RegularExpression("[A-Z][12][0-9]{8}", ErrorMessage = "身分證字號格式錯誤")]
        [MyValidator.TaiwanID]
        public string NationalID { get; set; } 

        [Display(Name = "電子郵件")]
        [Required(ErrorMessage = "必填")]
        [StringLength(40, ErrorMessage = "電子郵件最多40個字")]
        [EmailAddress(ErrorMessage = "電子郵件格式錯誤")]
        public string Email { get; set; } 

        [Display(Name = "分機")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "請輸入4碼")]
        [RegularExpression("[#][0-9]{3}", ErrorMessage = "分機格式錯誤 EX:#123")]
        public string? Extension { get; set; }

        [Display(Name = "照片")]
        [StringLength(10)]
        public IFormFile? PhotoFile { get; set; }

        // 🚩 用來存放最後存入資料庫的「檔名路徑」(字串)
        public string? Photo { get; set; }

        [Display(Name = "建立時間")]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日 hh:mm:ss}")]
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        [Display(Name = "最後登入時間")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日 hh:mm:ss}")]
        public DateTime? LastLoginTime { get; set; } = DateTime.Now;

        [Display(Name = "帳號")]
        [Required(ErrorMessage = "必填")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "帳號長度需介於 8 到 30 個字元")]
        [RegularExpression("[a-zA-Z0-9]*$", ErrorMessage = "帳號只能包含英文字母、數字")]
        public string Account { get; set; } 

        [Display(Name = "密碼")]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.Password)]
        [StringLength(200, MinimumLength = 8, ErrorMessage = "密碼長度至少需 8 位")]
        [RegularExpression("(?=.*[a-z])(?=.*[A-Z])(?=.[0-9]){8,}$", ErrorMessage = "密碼必須包含至少一個大寫字母、一個小寫字母與一個數字")]
        public string Password { get; set; } 

        public string AccountStatusID { get; set; }

        public string RoleID { get; set; } 


    }
}
