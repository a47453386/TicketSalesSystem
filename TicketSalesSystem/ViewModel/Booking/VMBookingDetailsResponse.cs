namespace TicketSalesSystem.ViewModel.Booking
{
    public class VMBookingDetailsResponse: VMBookingResponse
    {
        public List<VMTicketDetail> TicketDetails { get; set; } = new List<VMTicketDetail>();
    }
}
