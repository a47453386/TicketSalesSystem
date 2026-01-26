using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class FAQType
    {
        [Key]
        [Column(TypeName = "nchar(2)")]
        public string FAQTypeID { get; set; } = null!;

        [Display(Name = "常見問題種類")]
        [Required(ErrorMessage = "必填")]
        [StringLength(10, ErrorMessage = "種類名稱最多10個字")]
        public string FAQTypeName { get; set; } = null!;


        //關聯區
        public virtual List<FAQ>? FAQ { get; set; }

    }
}
