using System.ComponentModel.DataAnnotations;
using TicketSalesSystem.ViewModel.Programme.CreateProgramme.Item;
using TicketSalesSystem.ViewModel.Programme.Index;

namespace TicketSalesSystem.ViewModel.Programme
{
    public class VMProgramme
    {

        public string? EmployeeID { get; set; }

        [Key]
        public string ProgrammeID { get; set; } = null!;
        public string ProgrammeName { get; set; } = null!;
        public string? CoverImage { get; set; }
        public string PlaceID { get; set; } = null!;
        public string PlaceName { get; set; } = null!;


        public string SessionID { get; set; } = null!;
        public DateTime SaleStartTime { get; set; }
        public DateTime SaleEndTime { get; set; }
        public DateTime? StartTime { get; set; }




        public string ProgrammeStatusID { get; set; } = null!;
        public string ProgrammeStatusName { get; set; } = null!;


        public string FinalStatusText => ProgrammeStatusName;

        public int Capacity { get; set; }  // 總庫存
        public int Remaining { get; set; } // 總剩餘

        //已售出數量
        public int SoldCount => Capacity - Remaining;

        // 計算銷售百分比的唯讀屬性
        public int SalesPercent => Capacity > 0
            ? (int)((double)SoldCount / Capacity * 100)
            : 0;

        //顏色判斷
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

        public List<ProgrammeIndexSession> Session { get; set; } = new List<ProgrammeIndexSession>();


    }
}
