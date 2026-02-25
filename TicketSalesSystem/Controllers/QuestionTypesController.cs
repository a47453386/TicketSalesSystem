using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.ID;

namespace TicketSalesSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "EmployeeScheme", Roles = "S,A,C")]
    public class QuestionTypesController : Controller
    {
        private readonly TicketsContext _context;
        private readonly IIDService _idService;

        public QuestionTypesController(TicketsContext context, IIDService idService)
        {
            _context = context;
            _idService = idService;
        }

        // 1. 回傳局部檢視內容
        public IActionResult GetPartialList()
        {
            try
            {
                var list = _context.QuestionType.ToList();

                // 🚩 只要檔案在 Views/Shared/ 之下，這樣寫就能找到
                // 請確認檔名是否為 "_QuestionTypeManagerPartial" 或 "_QuestionTypeManager"
                return PartialView("_QuestionTypeManagerPartial", list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // 2. 快速新增的 API
        [HttpPost]
        public async Task<JsonResult> QuickCreate(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Json(new { success = false, message = "請輸入分類名稱" });
            }
            try
            {
                var newType = new QuestionType
                {
                    QuestionTypeID = await _idService.GetQuestionTypeID(),
                    QuestionTypeName = name
                };
                _context.Add(newType);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = errorMsg });
            }

        }

        

        // POST: QuestionTypes/Edit/5
        [HttpPost]
        public async Task<JsonResult > Edit(string id, string name)
        {
            var questionType = await _context.QuestionType.FindAsync(id);
            if (questionType == null)
            {
                return Json(new { success = false, message = "找不到該分類" });
            }

            try
            {
                questionType.QuestionTypeName = name;                
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = errorMsg });
            }


        }

        

        // POST: QuestionTypes/Delete/5
        [HttpPost]
      
        public async Task<JsonResult> Delete(string id)
        {
            var questionType = await _context.QuestionType.FindAsync(id);
            if (questionType == null)
            {
                return Json(new { success = false, message = "找不到該分類" });
            }
            
            try
            {
                _context.QuestionType.Remove(questionType);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = errorMsg });
            }
        }
        
    }
}
