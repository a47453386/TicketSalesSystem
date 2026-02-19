using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel.Programme.EditProgramme;

namespace TicketSalesSystem.Service.Validation.IProgrammeValidationService
{
    public class ProgrammeValidationService:IProgrammeValidationService
    {
        private readonly TicketsContext _context;
        public ProgrammeValidationService(TicketsContext context)
        {
            _context = context;
        }

        // 驗證票區容量是否足以容納已售出的票
        public async Task<(bool IsValid, string Message)> ValidateAreaCapacityAsync(string areaId, int newRowCount, int newSeatCount) 
        {
            if (string.IsNullOrEmpty(areaId)) return (true, ""); // 新增的票區不需要檢查
            int newCapacity = newRowCount * newSeatCount;
            //核心業務規則：檢查已售出的實體票券數
            int soldCount = await _context.Tickets
                .CountAsync(t => t.TicketsAreaID == areaId && t.Order.OrderStatusID != "N");

            if (newCapacity < soldCount)
            {
                return (false, $"容量不足：該區已售出 {soldCount} 張票，但新設定僅能容納 {newCapacity} 位。");
            }

            return (true, "");
        }

        
    }
}
