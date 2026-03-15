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
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 🚩 修正：必須預載入 Tickets 與 Session，否則 foreach 裡的 t.Session 會是 null
                var order = await _context.Order
                    .Include(o => o.Tickets)
                        .ThenInclude(t => t.Session)
                    .FirstOrDefaultAsync(o => o.OrderID == request.OrderID && o.OrderStatusID != "N");

                if (order == null) return (false, "訂單不存在或已過期。");

                // 1. 更新訂單狀態
                order.OrderStatusID = "Y"; // 已完款
                order.PaymentStatus = true;
                order.PaymentMethodID = request.PaymentMethodID;
                order.PaidTime = DateTime.Now;

                // 2. 處理票券狀態
                foreach (var t in order.Tickets)
                {
                    // 🚩 現在 t.Session 有資料了，可以正確判斷日期
                    if (t.Session != null && DateTime.Now >= t.Session.StartTime.AddDays(-15))
                    {
                        t.TicketsStatusID = "A"; // 未使用 (待領票/待進場)
                                                 // 產生 12 位核銷碼
                        t.CheckInCode = StringHelper.GenerateCheckInCode(12);
                    }
                    else
                    {
                        t.TicketsStatusID = "S"; // 已售出 (鎖定中)
                    }

                    
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "付款處理成功。");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "處理失敗：" + ex.Message);
            }
        }
    }




}

