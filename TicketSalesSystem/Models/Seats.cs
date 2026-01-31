using Microsoft.CodeAnalysis.Elfie.Serialization;
using System.ComponentModel.DataAnnotations;

namespace TicketSalesSystem.Models
{
    public class Seats
    {
        [Key]
        public string AreaID { get; set; } = null!;

        public int SeatIndex { get; set; }

        public int rowIndex { get; set; }
        public string RowName { get; set; } = null!;

        public string SeatName { get; set; }= null!;

        public string VenueID { get; set; } = null!;

        // 邏輯 ID：由區域、排、位組成，例如 "V01_A_12"
        // 這樣前端回傳這個字串，你就能解析出它是哪一個位子
        public string SeatIdentifier => $"{AreaID}_{RowName}_{SeatName}";

        // 狀態（動態比對後填入）
        public string Status { get; set; } = "Available";

        
    }
}
