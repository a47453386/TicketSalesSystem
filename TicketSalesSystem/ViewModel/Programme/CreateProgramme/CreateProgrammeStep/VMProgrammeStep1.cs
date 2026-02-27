
using System.ComponentModel.DataAnnotations;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.ViewModel.Programme.CreateProgramme.CreateProgrammeStep
{
    public class VMProgrammeStep1
    {
        [Key]
        
        public string ProgrammeName { get; set; } = null!;
        
        public string ProgrammeDescription { get; set; } = null!;
        public string Notice { get; set; } = null!;
        public string PurchaseReminder { get; set; } = null!;
        public string CollectionReminder { get; set; } = null!;
        public string RefundPolicy { get; set; } = null!;

        [Display(Name = "限購")]
        [Required(ErrorMessage = "請輸入限購張數")]
        [Range(0, 6, ErrorMessage = "限購數量請設定在 0 到 6 之間")]
        public int? LimitPerOrder { get; set; }
        
        public DateTime OnShelfTime { get; set; }
       
        public string? PlaceID { get; set; }

        public string? ProgrammeStatusID { get; set; }
    }
}
