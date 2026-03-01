namespace TicketSalesSystem.ViewModel.Programme.ProgrammeAdminDetail
{
    public class VMAreaDetail
    {
        public string TicketsAreaID { get; set; } = null!;
        public string TicketsAreaName { get; set; } = null!;
        public decimal? Price { get; set; }
        public int Capacity { get; set; }
        public int Sold { get; set; } // 🚩 關鍵：即時計算的銷售量
        public int Remaining { get; set; }
        public int? RowCount { get; set; }
        public int? SeatCount { get; set; }
    }
}
