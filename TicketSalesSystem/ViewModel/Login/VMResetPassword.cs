using System.ComponentModel.DataAnnotations;

namespace TicketSalesSystem.ViewModel.Login
{
    public class VMResetPassword
    {
        // 🚩 核心：儲存那串長長的加密字串 (從網址列抓取)
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入新通行碼")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "密碼長度至少需要 6 位")]
        [Display(Name = "新密碼")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "請再次確認新通行碼")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "兩次輸入的密碼不一致")]
        [Display(Name = "確認新密碼")]
        public string ConfirmPassword { get; set; } = string.Empty;


    }
}
