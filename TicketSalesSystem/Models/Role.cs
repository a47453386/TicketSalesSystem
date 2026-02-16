using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class Role
    {
        [Key]
        [StringLength(1)]
        public string RoleID { get; set; } = null!;

        [Display(Name = "職位名稱")]
        [Required(ErrorMessage = "必填")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "職位名稱2~20個字")]
        public string RoleName { get; set; } = null!;

        [Display(Name = "職位說明")]
        [DataType(DataType.MultilineText)]
        public string? RoleDescription { get; set; }




        //關聯區
        public virtual List<Employee>? Employee { get; set; }



        //顯示用
        [NotMapped]
        public string FullDisplayName => $"{RoleID} - {RoleName}";
    }
}
