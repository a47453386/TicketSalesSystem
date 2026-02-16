namespace TicketSalesSystem.ViewModel.Programme.CreateProgramme.Item
{
    public class VMCreateConfirm
    {
        // Step 1 的資料
        public string ProgrammeName { get; set; }
        public string CategoryName { get; set; } // 顯示名稱而非 ID
        public string PlaceName { get; set; }    // 顯示名稱而非 ID

        // Step 2 的場次
        public List<VMSessionItem> Session { get; set; }

        // Step 3 的場地與票區
        public string VenueName { get; set; }    // 顯示名稱而非 ID
        public List<VMTicketsAreaItem> TicketsArea { get; set; }
    }
}
