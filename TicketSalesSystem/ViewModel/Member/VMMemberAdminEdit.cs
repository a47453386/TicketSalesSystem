using System.ComponentModel.DataAnnotations;
using static TicketSalesSystem.ValidationAttributes.MyValidator;

namespace TicketSalesSystem.ViewModel.Member
{
    public class VMMemberAdminEdit: VMMemberUserEdit
    {
        [Display(Name = "會員姓名")]
        [Required(ErrorMessage = "必填")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "請輸入2~20個字")]
        public string Name { get; set; } = null!;

        [Display(Name = "生日")]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日}")]
        public DateTime Birthday { get; set; }

        [Display(Name = "性別")]
        [Required(ErrorMessage = "必填")]
        public bool Gender { get; set; }

        [Display(Name = "身分證字號")]
        [Required(ErrorMessage = "必填")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "請輸入10碼")]
        [RegularExpression("[A-Z][12][0-9]{8}", ErrorMessage = "身分證字號格式錯誤")]
        [TaiwanID]
        public string NationalID { get; set; }

        [Display(Name = "手機驗證狀態")]
        [Required(ErrorMessage = "必填")]
        public bool IsPhoneVerified { get; set; }

        [Display(Name = "帳號狀態")]
        public string AccountStatusID { get; set; }

        public string Account { get; set; } = null!;

    }
}
