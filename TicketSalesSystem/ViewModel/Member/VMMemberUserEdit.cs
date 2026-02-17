using System.ComponentModel.DataAnnotations;
using static TicketSalesSystem.ValidationAttributes.MyValidator;

namespace TicketSalesSystem.ViewModel.Member
{
    public class VMMemberUserEdit
    {
        [Key]
        [Display(Name = "會員編號")]
        public string MemberID { get; set; }= null!;

        [Display(Name = "會員姓名")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "請輸入2~20個字")]
        public string Name { get; set; } = null!;

        [Display(Name = "會員地址")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "請輸入2~50個字")]
        public string Address { get; set; }

        [Display(Name = "生日")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日}")]
        public DateTime Birthday { get; set; } 

        [Display(Name = "手機電話")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "請輸入10碼")]
        [RegularExpression("09[0-9]{8}", ErrorMessage = "手機格式錯誤，請輸入 09 開頭的 10 位數字")]
        [MemberTelDuplicateCheck]
        public string Tel { get; set; }

        [Display(Name = "性別")]
        [Required(ErrorMessage = "必填")]
        public bool Gender { get; set; } 

        [Display(Name = "身分證字號")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "請輸入10碼")]
        [RegularExpression("[A-Z][12][0-9]{8}", ErrorMessage = "身分證字號格式錯誤")]
        [TaiwanID]
        public string NationalID { get; set; } 


        [Display(Name = "電子郵件")]
        
        [StringLength(40, ErrorMessage = "電子郵件最多40個字")]
        [EmailAddress(ErrorMessage = "電子郵件格式錯誤")]
        public string Email { get; set; }


        


        [Display(Name = "帳號")]
        public string Account { get; set; } 
        [Display(Name = "密碼")]
        public string Password { get; set; }


       

    }
}
