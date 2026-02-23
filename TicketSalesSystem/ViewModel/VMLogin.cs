using System.ComponentModel.DataAnnotations;

namespace TicketSalesSystem.ViewModel
{
    public class VMLogin
    {
        [Display(Name = "帳號")]
        [Required(ErrorMessage = "請輸入帳號")]
        public string Account { get; set; } = null!;

        [Display(Name = "密碼")]
        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        // 可以加一個記住我，測試時很好用
        public bool RememberMe { get; set; }

    }
}
