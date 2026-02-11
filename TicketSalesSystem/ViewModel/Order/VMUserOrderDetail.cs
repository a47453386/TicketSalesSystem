namespace TicketSalesSystem.ViewModel.Order
{
    public class VMUserOrderDetail
    {
        public string OrderID { get; set; }
        public string ProgrammeName { get; set; }
        public decimal FinalAmount { get; set; }
        public string StartTime { get; set; } // 格式化好的時間
        
        public string PlaceName { get; set; }
        public string OrderStatusName { get; set; }
        public bool IsPrintable { get; set; } // 是否已進入 15 天內的發票期
        public List<VMUserTicketItem> Tickets { get; set; }

        public List<string> Seats { get; set; } = new List<string>();
    }
}
