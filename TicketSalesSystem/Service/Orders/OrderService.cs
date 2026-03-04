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
            // 執行「原子性更新」(Atomic Update)
            // 只有在狀態依然是暫存狀態(例如 'W' 或 'O')時才更新，這能完美避開背景服務的競爭
            var affectedOrders = await _context.Order
                   .Where(o => o.OrderID == request.OrderID && o.OrderStatusID != "N")
                   .ExecuteUpdateAsync(s => s
                       .SetProperty(o => o.OrderStatusID, "Y")// 改為已完成訂單  
                       .SetProperty(o => o.PaymentStatus, true)// 改為已付款
                       .SetProperty(o => o.PaymentMethodID, request.PaymentMethodID));

            if (affectedOrders == 0)
            {
                return (false, "訂單狀態不符合付款條件或已逾時。");
            }

            // 撈取該訂單關聯的所有票券 (目前的 TicketsStatusID 應該還是 'P')
            var tickets = await _context.Tickets
                .Where(t => t.OrderID == request.OrderID)
                .ToListAsync();

            //逐一執行「出票」邏輯：改狀態、產碼
            foreach (var t in tickets)
            {
                t.TicketsStatusID = "S"; // Sold (已售出)

                // 🚩 使用 Helper 進行 GUID 擷取 12 碼
                t.CheckInCode = StringHelper.GenerateCheckInCode(12);
            }

            //儲存票券狀態與核銷碼
            await _context.SaveChangesAsync();

            return (true, "付款處理成功，票券已核發。");

        }

       


    }
}
