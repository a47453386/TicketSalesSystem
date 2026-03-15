using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.DTOs.Question
{
  // 你的 DTO 維持這樣就好
public class QuestionCreateDto
{
    [Required(ErrorMessage = "主旨不可空白")]
    public string QuestionTitle { get; set; }

    [Required(ErrorMessage = "內容不可空白")]
    public string QuestionDescription { get; set; }
   
   [Required]
    public string QuestionTypeID { get; set; } // 🚩 傳入 T01, T02 等 ID 即可
}
}
