using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class FAQ
    {
        [Key]
        [StringLength(36, MinimumLength = 36)]
        [HiddenInput]
        public string FAQID { get; set; } = null!;


        [Display(Name = "常見問題")]
        [Required(ErrorMessage = "必填")]
        [StringLength(50, ErrorMessage = "標題最多50個字")]
        public string FAQTitle { get; set; } = null!;


        [Display(Name = "常見問題說明")]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.MultilineText)]
        public string FAQDescription { get; set; } = null!;





        //FK區
        [Display(Name = "負責人員")]
        public string EmployeeID { get; set; } = null!;
        [Display(Name = "發佈狀態")]
        public string FAQPublishStatusID { get; set; } = null!;
        [Display(Name = "常見問題種類")]
        public string FAQTypeID { get; set; } = null!;

        //關聯區
        public virtual FAQPublishStatus? FAQPublishStatus { get; set; }
        public virtual Employee? Employee { get; set; }
        public virtual FAQType? FAQType { get; set; }
    }
}
