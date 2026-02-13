using System.ComponentModel.DataAnnotations;

namespace TicketSalesSystem.Models
{
    public class PublicNotice
    {
        [Key]
        [StringLength(36, MinimumLength = 36)]
        public string PublicNoticeID { get; set; } = null!;//採用GUID

        [Display(Name = "公告標題")]
        [Required(ErrorMessage = "必填")]
        [StringLength(50, ErrorMessage = "公告標題最多50個字")]
        public string PublicNoticeTitle { get; set; } = null!;

        [Display(Name = "公告內容")]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.MultilineText)]
        public string PublicNoticeDescription { get; set; } = null!;

        [Display(Name = "建立時間")]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日 HH:mm}")]
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        [Display(Name = "更新時間")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日 HH:mm}")]
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;


        [Display(Name = "下架時間")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日 HH:mm}")]
        public DateTime? RemovalTime { get; set; }


        [Display(Name = "發佈時間")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy年MM月dd日 HH:mm}")]
        public DateTime? PublishTime { get; set; }
        

        [Display(Name = "發佈狀態")]
        public bool PublicNoticeStatus { get; set; } =false;




        //FK區
        [Display(Name = "負責人")]
        public virtual string EmployeeID { get; set; } = null!;


        //關聯區
        public virtual Employee? Employee { get; set; }
    }
}

