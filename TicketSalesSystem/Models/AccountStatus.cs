using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class AccountStatus
    {
        [Key]
        [Column(TypeName = "nchar(1)")]
        public string AccountStatusID { get; set; }= null!;

        [Display(Name = "帳號狀態名稱")]
        [Required(ErrorMessage = "必填")]
        [StringLength(10, ErrorMessage = "帳號狀態名稱最多10個字")]
        public string AccountStatusName { get; set; }= null!;



        //關聯
        public List<Member>? Member { get; set; }
        public List<Employee>? Employee { get; set; }
    }

}
