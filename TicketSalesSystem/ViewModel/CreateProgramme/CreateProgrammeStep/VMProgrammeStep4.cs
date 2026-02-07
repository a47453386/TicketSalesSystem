using System.ComponentModel.DataAnnotations;
using TicketSalesSystem.ViewModel.CreateProgramme.Item;

namespace TicketSalesSystem.ViewModel.CreateProgramme.CreateProgrammeStep
{
    public class VMProgrammeStep4
    {

        // 用來接收上傳的實體檔案
        public IFormFile? CoverImageFile { get; set; }
        public IFormFile? SeatImageFile { get; set; }
        public  List<IFormFile> DescriptionImageFiles { get; set; }

        [Key]
        // 原本的 string 留著，用來回顯已經上傳過的檔案路徑
        public string? CoverImage { get; set; }

        public string? SeatImage { get; set; }

        public virtual List<VMDescriptionImageItem> DescriptionImages { get; set; } = new List<VMDescriptionImageItem>();
    }
}
