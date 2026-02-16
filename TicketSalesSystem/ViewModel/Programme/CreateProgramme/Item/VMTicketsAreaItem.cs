namespace TicketSalesSystem.ViewModel.Programme.CreateProgramme.Item
{
    public class VMTicketsAreaItem
    {
        public string? TicketsAreaID { get; set; } 
        public string TicketsAreaName { get; set; } = null!;

        public int RowCount { get; set; }

        public int SeatCount { get; set; }

        public decimal Price { get; set; }


        public string? VenueID { get; set; } 
        public string? TicketsAreaStatusID { get; set; } 

        public string TempId { get; set; } = Guid.NewGuid().ToString();
    }
        
}
