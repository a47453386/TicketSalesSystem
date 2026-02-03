namespace TicketSalesSystem.ViewModel
{
    public class VMBookingRequest
    {
        public string SessionID { get; set; } = null!;

        public string AreaID { get; set; } = null!;

        public string MemberID { get; set; } = null!;

        public string PaymentMethodID { get; set; } = null!;        

        public int Count { get; set; }

        public decimal TotalAmount { get; set; }



        public List<string> Seats { get; set; } 
    }
}
