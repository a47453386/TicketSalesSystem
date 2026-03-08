using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Helpers;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel.Booking;

namespace TicketSalesSystem.Service.Orders
{
    public class OrderService: IOrderService
    {
        private readonly TicketsContext _context;
        public OrderService(TicketsContext context)
        {
            _context = context;
        }

        public async Task<VMBookingResponse> GetPaymentDetailsAsync(string orderId)
        {
            var order = await _context.Order
                .Include(o => o.Session).ThenInclude(s => s.Programme).ThenInclude(p => p.Place)
                .Include(o => o.Tickets).ThenInclude(t => t.TicketsArea)
                .FirstOrDefaultAsync(o => o.OrderID == orderId);


            // 計算到期時間 (訂單建立時間 + 10 分鐘)
            var expireTime = order.OrderCreatedTime.AddMinutes(10);
            var remainingSeconds = (int)(expireTime - DateTime.Now).TotalSeconds;

            return new VMBookingResponse
            {
                Success = true,
                OrderID = order.OrderID,
                ProgrammeName = order.Session.Programme.ProgrammeName,
                StartTime = order.Session.StartTime.ToString("yyyy-MM-dd HH:mm"),
                PlaceName = order.Session.Programme.Place.PlaceName,
                FinalAmount = order.Tickets.Sum(t => t.TicketsArea.Price),
                Seats = order.Tickets.Select(t => $"{t.TicketsArea.TicketsAreaName} 區 {t.RowIndex} 排{t.SeatIndex} 號").ToList(),
                RemainingSeconds = remainingSeconds > 0 ? remainingSeconds : 0,
                ExpireTimeText = expireTime.ToString("yyyy-MM-ddTHH:mm:ss")
            };
        }



        // 處理支付完成後的邏輯(正式結帳與電子票券核發)
        public async Task<(bool Success, string Message)> ProcessPaymentAsync(VMPaymentRequest request)
        {
            // 🚩 1. 使用 Transaction 確保原子性
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. 更新訂單狀態 (改用傳統 EF 寫法，確保追蹤一致)
                var order = await _context.Order
                    .FirstOrDefaultAsync(o => o.OrderID == request.OrderID && o.OrderStatusID != "N");

                if (order == null) return (false, "訂單狀態不符合付款條件或已逾時。");

                order.OrderStatusID = "Y";
                order.PaymentStatus = true;
                order.PaymentMethodID = request.PaymentMethodID;
                order.PaidTime = DateTime.Now;

                // 🚩 3. 立即處理票券
                var tickets = await _context.Tickets
                    .Where(t => t.OrderID == request.OrderID)
                    .ToListAsync();

                foreach (var t in tickets)
                {
                    t.TicketsStatusID = "A"; // 未使用
                    t.CheckInCode = StringHelper.GenerateCheckInCode(12);
                }

                // 🚩 4. 一次性儲存所有變更 (包含 Order 跟 Tickets)
                await _context.SaveChangesAsync();

                // 🚩 5. 提交交易
                await transaction.CommitAsync();

                return (true, "付款處理成功，票券已核發。");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // 失敗就全部重來
                return (false, "系統錯誤：" + ex.Message);
            }
        }




    }
}
