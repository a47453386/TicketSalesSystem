namespace TicketSalesSystem.DTOs.Login
{
    public class LoginResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string MemberID { get; set; }

        public string Name { get; set; }
    }
}
