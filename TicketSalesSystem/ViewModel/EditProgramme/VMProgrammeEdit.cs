using Microsoft.AspNetCore.Mvc.Rendering;
using TicketSalesSystem.DTOs;
using TicketSalesSystem.ViewModel.CreateProgramme.Item;

namespace TicketSalesSystem.ViewModel.EditProgramme
{
    public class VMProgrammeEdit
    {
        // --- 活動主表資料 ---
        public string ProgrammeID { get; set; } = null!;
        public string ProgrammeName { get; set; } = null!;
        public string ProgrammeDescription { get; set; } = null!;
        public string? CoverImage { get; set; }
        public string? SeatImage { get; set; }
        public int? LimitPerOrder { get; set; }
        public DateTime OnShelfTime { get; set; }
        public string? PlaceID { get; set; }

        public string? ProgrammeStatusID { get; set; }
        public string? VenueID { get; set; } = null!;
        public string? TicketsAreaStatusID { get; set; } = null!;

        // --- 關聯資料 (一次全部載入) ---
        public List<DescriptionImageDTO> DescriptionImage { get; set; } = new List<DescriptionImageDTO>();
        public List<VMSessionItem>? Session { get; set; } = new List<VMSessionItem>();
        public List<TicketsAreaDTO> TicketsArea { get; set; } = new List<TicketsAreaDTO>();

        // --- 新增這兩個屬性，前端 <input type="file"> 才能對應到 ---
        public IFormFile? CoverImageFile { get; set; }
        public IFormFile? SeatImageFile { get; set; }
        

        public List<string>? DeleteImageIds { get; set; }// 關鍵：用來接收前端勾選「要刪除」的圖片 ID 清單
        public List<IFormFile>? DescriptionImageFiles { get; set; }
        public List<IFormFile>? NewDescriptionFiles { get; set; } 
    }
}
