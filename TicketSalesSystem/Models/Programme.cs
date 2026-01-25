using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.Models
{
    public class Programme
    {
        [Key]
        [Column(TypeName = "nchar(8)")]
        public string ProgrammeID { get; set; } = null!;

        [Display(Name = "活動名稱")]        
        [Required(ErrorMessage = "必填")]
        [StringLength(50, ErrorMessage = "活動名稱最多50個字")]
        public string ProgrammeName { get; set; } = null!;

        [Display(Name = "活動內容")]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.MultilineText)]
        public string ProgrammeDescription { get; set; } = null!;


        [Display(Name = "建立時間")]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd hh:mm:ss}")]
        public DateTime CreatedTime { get; set; } = DateTime.Now;


        [Display(Name = "更新時間")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd hh:mm:ss}")]
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;


        [Display(Name = "封面圖片")]
        [Required(ErrorMessage = "必填")]
        [StringLength(40, MinimumLength = 40)]
        public string CoverImage { get; set; } = null!;

   
        [Display(Name = "座位圖片")]
        [Required(ErrorMessage = "必填")]
        [StringLength(13, MinimumLength = 13)]
        public string SeatImage { get; set; } = null!;

        [Display(Name = "限購")]
        [Required(ErrorMessage = "必填")]
        [Range(0, 6,ErrorMessage ="請輸入0-6數字")]
        public int? LimitPerOrder { get; set; }



        //FK區
        public string EmployeeID { get; set; } = null!;
        public string PlaceID { get; set; } = null!;
        public string ProgrammeStatusID { get; set; } = null!;


        //關聯區
        public virtual Employee? Employee { get; set; }        
        public virtual Place? Place { get; set; }                     
        public virtual ProgrammeStatus? ProgrammeStatus { get; set; }


        public virtual List<DescriptionImage>? DescriptionImage { get; set; }
        public virtual List<Session>? Session { get; set; }
        public virtual List<Order>? Order { get; set; }
    }
}
