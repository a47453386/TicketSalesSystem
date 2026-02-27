namespace TicketSalesSystem.DTOs.PutDTO
{
    public class ProgrammePutDTO
    {
        public string ProgrammeName { get; set; }        
        public string ProgrammeDescription { get; set; }
        public string Notice { get; set; } 
        public string PurchaseReminder { get; set; } 
        public string CollectionReminder { get; set; } 
        public string RefundPolicy { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public string? SeatImage { get; set; }
        public string? CoverImage { get; set; }
           
        public int LimitPerOrder { get; set; }

        public DateTime? OnShelfTime { get; set; }

        public string PlaceID { get; set; }

        public string? ProgrammeStatusID { get; set; }
  
        
        // 關聯子集合
        public List<SessionPutDTO> Session { get; set; } = new List<SessionPutDTO>();

        // 處理圖片刪除
        public List<string> DeleteImageID { get; set; } = new List<string>();
    }
}
