using System.ComponentModel.DataAnnotations;

namespace TicketSalesSystem.ViewModel.Programme.Index
{
    public class VMprogrammeIndex
    {
        [Key]
        public string ProgrammeID { get; set; } = null!;
        public string ProgrammeName { get; set; } = null!;
        public string? CoverImage { get; set; }
        public string PlaceName { get; set; } = "尚未公佈地點";

        // 狀態相關
        public string ProgrammeStatusID { get; set; } = null!;
        public string ProgrammeStatusName { get; set; } = "售票中";

        // 票數計算
        public int Capacity { get; set; }
        public int Remaining { get; set; }
        public int SoldCount => Capacity - Remaining;
        public int SalesPercent => Capacity > 0 ? (int)((double)SoldCount / Capacity * 100) : 0;

        // 🚩 重點：確保 List 在建構時就初始化，防止 NullReference
        public List<ProgrammeIndexSession> Session { get; set; } = new List<ProgrammeIndexSession>();

        // 顏色判斷 (保持你原本的邏輯)
        public string StatusColor => ProgrammeStatusID switch
        {
            "D" => "warning",   // Draft (草稿) - 黃色，提醒尚未發布
            "R" => "info",      // Ready (設定完成/即將開賣) - 淺藍色
            "O" => "success",   // OnSale (售票中) - 綠色
            "S" => "dark",      // Sold Out (已售完) - 黑色或深灰色，代表完售
            "H" => "light",     // Hidden (已下架) - 淺色，低調顯示
            "E" => "secondary", // Ended (已結束) - 灰色
            "C" => "danger",    // Canceled (已取消) - 紅色
            _ => "primary"      // 預設 - 藍色
        };
    }
}
