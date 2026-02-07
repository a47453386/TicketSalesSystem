namespace TicketSalesSystem.ViewModel.CreateProgramme.Item
{
    public class VMTicketsAreaItem
    {
        public string TicketsAreaName { get; set; } = null!;

        public int RowCount { get; set; }

        public int SeatCount { get; set; }

        public decimal Price { get; set; }

       
        public string TempId { get; set; } = Guid.NewGuid().ToString();
    }
        
}
