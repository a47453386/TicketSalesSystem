using System.ComponentModel.DataAnnotations;

namespace TicketSalesSystem.ViewModel
{
    public class VMBookingResponse
    {       
            
            public bool Success { get; set; }
            public string Message { get; set; } = "";
            public string OrderID { get; set; }
            public List<string> Seats { get; set; } = new List<string>();
            public decimal FinalAmount { get; set; }
            public int RemainingSeconds { get; set; }//保留時間

            public string ExpireTimeText { get; set; } = "";//倒數計時顯示器
        
    }
}
