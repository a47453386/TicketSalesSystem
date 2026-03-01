namespace TicketSalesSystem.ViewModel.Programme.ProgrammeAdminDetail
{
    public class VMSessionDetail
    {
        public string SessionID { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public List<VMAreaDetail> TicketsAreas { get; set; } = new();

        // 該場次總結 (用於計算總進度)
        public int TotalCapacity => TicketsAreas.Sum(a => a.Capacity);
        public int TotalSold => TicketsAreas.Sum(a => a.Sold);
    }
}
