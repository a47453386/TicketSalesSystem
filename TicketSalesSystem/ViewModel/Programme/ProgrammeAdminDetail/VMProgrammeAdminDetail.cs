using TicketSalesSystem.Models;

namespace TicketSalesSystem.ViewModel.Programme.ProgrammeAdminDetail
{
    public class VMProgrammeAdminDetail
    {
        public string ProgrammeID { get; set; } = null!;
        public string ProgrammeName { get; set; } = null!;
        public string? StatusName { get; set; } // 顯示 "上架中" 等文字
        public string? PlaceName { get; set; }
        public string? ProgrammeDescription { get; set; }
        public string? Notice { get; set; }
        public string? RefundPolicy { get; set; }
        public string? CoverImage { get; set; }
        public string? SeatImage { get; set; }
        
        public DateTime OnShelfTime { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? EmployeeName { get; set; } // 建立者名稱

       
        public List<VMSessionDetail> Sessions { get; set; } = new();
        public List<string> DescriptionImages { get; set; } = new();
    }

    
}
