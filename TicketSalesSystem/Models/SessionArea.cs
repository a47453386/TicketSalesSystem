using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class SessionArea
    {
        //PK區
        [Key]
        [ForeignKey("Session")]
        public string SessionID{ get; set; } = null!;

        [Key]
        [ForeignKey("TicketsArea")]
        public string TicketsAreaID { get; set; } = null!;



        [Display(Name = "票價")]
        [Required(ErrorMessage = "必填")]
        [Column(TypeName = "Money")]
        [Range(0, 100000, ErrorMessage = "票價必須在 0 到 100,000 之間")]
        public decimal Price { get; set; }

        //關聯區
        public virtual Session? Session { get; set; }
        public virtual TicketsArea? TicketsArea { get; set; }
    }
}
