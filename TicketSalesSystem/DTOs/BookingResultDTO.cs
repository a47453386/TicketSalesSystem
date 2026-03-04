namespace TicketSalesSystem.DTOs
{
    public class BookingResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; } // 用來放訂單編號等資訊
        public bool ShouldRedirectToLogin { get; set; } // 讓 Controller 判斷是否要跳轉

    }
}
