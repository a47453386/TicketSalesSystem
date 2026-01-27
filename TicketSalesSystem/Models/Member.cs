using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class Member
    {
        [Key]
        [Display(Name = "會員編號")]
        [StringLength(36, MinimumLength = 36)]        
        public string MemberID { get; set; } = null!;

        [Display(Name = "會員姓名")]
        [Required(ErrorMessage = "必填")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "請輸入2~20個字")]
        public string Name { get; set; } = null!;

        [Display(Name = "會員地址")]
        [Required(ErrorMessage = "必填")]
        [StringLength(50, MinimumLength =2, ErrorMessage = "請輸入2~50個字")]
        public string Address { get; set; } = null!;

        [Display(Name = "生日")]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日}")]
        public DateTime Birthday { get; set; }

        [Display(Name = "手機電話")]
        [Required(ErrorMessage = "必填")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "請輸入10碼")]
        [RegularExpression("09[0-9]{8}", ErrorMessage = "手機格式錯誤，請輸入 09 開頭的 10 位數字")]
        public string Tel { get; set; } = null!;

        [Display(Name = "性別")]
        [Required(ErrorMessage = "必填")]
        public bool Gender { get; set; }

        [Display(Name = "身分證字號")]
        [Required(ErrorMessage = "必填")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "請輸入10碼")]
        [RegularExpression("[A-Z][12][0-9]{8}", ErrorMessage = "身分證字號格式錯誤")]
        public string NationalID { get; set; } = null!;


        [Display(Name = "電子郵件")]
        [Required(ErrorMessage = "必填")]
        [StringLength(40, ErrorMessage = "電子郵件最多40個字")]
        [EmailAddress(ErrorMessage = "電子郵件格式錯誤")]
        public string Email { get; set; } = null!;

        [Display(Name = "建立時間")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日 hh:mm:ss}")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "最後登入時間")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日 hh:mm:ss}")]
        public DateTime? LastLoginTime { get; set; } = DateTime.Now;

        [Display(Name = "手機是否驗證")]
        [Required(ErrorMessage = "必填")]
        public bool IsPhoneVerified { get; set; }




        //FK        
        public string AccountStatusID { get; set; } = null!;



        //關聯
        public virtual AccountStatus? AccountStatus { get; set; }
        public virtual MemberLogin? MemberLogin { get; set; }

        
        public virtual List<Order>? Order { get; set; }
        public virtual List<Question>? Question { get; set; }
    }
}
