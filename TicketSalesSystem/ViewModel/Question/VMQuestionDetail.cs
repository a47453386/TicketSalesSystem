using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSalesSystem.ViewModel.Question
{
    public class VMQuestionDetail
    {
        public string? OrderID { get; set; }
        public string? QuestionID { get; set; }
        public string? QuestionTitle { get; set; }
        public string? QuestionDescription { get; set; }
        public DateTime CreatedTime { get; set; }
        public string? UploadFile { get; set; }
        public string? QuestionTypeName { get; set; }

        // 🚩 改用 List 承接多筆回覆，避免資料遺失
        public List<VMReply> Reply { get; set; } = new();
    }
}
