using System.ComponentModel.DataAnnotations;

namespace TicketSalesSystem.ViewModel.CreateProgramme.Item
{
    public class VMSessionItem
    {
        [Required]
        [Display(Name = "場次時間")]
        public DateTime StartTime { get; set; }
        public DateTime SaleStartTime { get; set; }
        public DateTime SaleEndTime { get; set; }      

        // 可以在這裡加一個 TempId，方便前端刪除特定場次時使用
        public string TempId { get; set; } = Guid.NewGuid().ToString();
    }
}
