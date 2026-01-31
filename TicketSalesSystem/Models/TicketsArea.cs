using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class TicketsArea
    {
        [Key]
        [Column(TypeName = "nchar(3)")]
        public string TicketsAreaID { get; set; } = null!;

        [Display(Name = "票區")]
        [Required(ErrorMessage = "必填")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "請輸入2~20個字")]
        public string TicketsAreaName { get; set; } = null!;
        


        //FK
        public string TicketsAreaStatusID { get; set; } = null!;
        public string VenueID { get; set; } = null!;
        public string ProgrammeID { get; set; } = null!;

        public string SessionID { get; set; } = null!;

        //關聯
        public virtual TicketsAreaStatus? TicketsAreaStatus { get; set; }        
        public virtual Venue? Venue { get; set; }
        public virtual Programme? Programme { get; set; }

        public virtual Session? Session { get; set; }

        public virtual List<SessionArea>? SessionArea { get; set; }
    }
}
