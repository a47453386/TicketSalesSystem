using System.ComponentModel.DataAnnotations;

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



        //顏色判斷
        public string StatusColor => ProgrammeStatusID switch
        {
            "Upcoming" => "info",     // 即將開賣
            "OnSale" => "success",  // 售票中
            "Ended" => "secondary",// 已結束
            "Canceled" => "danger",   // 已取消
            _ => "primary"
        };




    }
}
