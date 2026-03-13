using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Helpers;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel;
using TicketSalesSystem.ViewModel.Booking;

namespace TicketSalesSystem.Service.Seats
{
    public class SeatService : ISeatService
    {
        private readonly TicketsContext _context;

        public SeatService(TicketsContext context)
        {
            _context = context;
        }

        // 1. 取得場次列表
        public async Task<List<Session>> GetSessionsAsync()
        {
            return await _context.Session.ToListAsync();
        }

        // 2. 同步票區狀態 (實作介面要求的方法)
        public async Task SyncAreaStatusAsync(string areaId)
        {
            var area = await _context.TicketsArea.FindAsync(areaId);
            if (area == null) return;

            // 根據 Remaining 欄位判斷 (由 SQL Trigger 維護)
            bool hasAvailableSeats = area.Remaining > 0;

            if (hasAvailableSeats && area.TicketsAreaStatusID == "O")
            {
                area.TicketsAreaStatusID = "I";
            }
            else if (!hasAvailableSeats && area.TicketsAreaStatusID == "I")
            {
                area.TicketsAreaStatusID = "O";
            }

            await _context.SaveChangesAsync();
        }

        // 3. 取得特定票區的座位圖 (供前端顯示)
        public async Task<List<VMSeats>> GetAreaLayoutAsync(string areaId)
        {
            var area = await _context.TicketsArea.FindAsync(areaId);
            if (area == null) return null;

            var soldTickets = await _context.Tickets
                .Where(t => t.TicketsAreaID == areaId)
                .ToListAsync();

            var layout = SeatHelper.GenerateSeatLayout(area.RowCount, area.SeatCount, soldTickets, area.TicketsAreaStatusID);
            return layout;
        }

        // 4. 取得場次的所有票區資訊
        public async Task<IEnumerable<object>> GetAreasBySession(string sessionId)
        {
            return await _context.TicketsArea
                .Where(a => a.SessionID == sessionId)
                .Select(a => new
                {
                    a.TicketsAreaID,
                    a.TicketsAreaName,
                    a.RowCount,
                    a.SeatCount,
                    a.Price,
                    a.Remaining,
                    a.TicketsAreaStatusID
                })
                .ToListAsync();
        }

        // 5. 建立訂單與票券 (核心購票方法)
        public async Task<VMBookingResponse> CreateOrderAndTicketsAsync(VMBookingRequest request, string memberID)
        {
            if (_context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
            {
                await _context.Database.OpenConnectionAsync();
            }

            // 🚩 交易保護：參與現有交易或開啟新交易
            using var transaction = _context.Database.CurrentTransaction != null
                ? null
                : await _context.Database.BeginTransactionAsync();

            try
            {
                var area = await _context.TicketsArea.FindAsync(request.TicketsAreaID);
                if (area == null || (area.TicketsAreaStatusID != "A" && area.TicketsAreaStatusID != "I"))
                {
                    return new VMBookingResponse
                    {
                        Success = false,
                        Message = area?.TicketsAreaStatusID == "O" ? "該區域已完售！" : "票區未開放購票"
                    };
                }

                // 自動配位
                var bestSeatIDs = await GetBestSeatsAsync(area.SessionID, area.TicketsAreaID, request.Count);
                if (!bestSeatIDs.Any())
                {
                    return new VMBookingResponse { Success = false, Message = "剩餘連續座位不足，請更換票區" };
                }

                // 取得自動編號
                var orderIDResult = await _context.Database.SqlQueryRaw<string>("SELECT dbo.funGetOrderID()").ToListAsync();
                string orderID = orderIDResult.FirstOrDefault();

                // 建立實體 (Order, Tickets)
                _context.Order.Add(new Order
                {
                    OrderID = orderID,
                    OrderCreatedTime = DateTime.Now,
                    PaymentTradeNO = Guid.NewGuid().ToString(),
                    PaymentStatus = false,
                    MemberID = memberID,
                    SessionID = request.SessionID,
                    OrderStatusID = "P",
                    PaymentMethodID = "A"
                });

                foreach (var seatID in bestSeatIDs)
                {
                    var parts = seatID.Split('-');
                    _context.Tickets.Add(new Tickets
                    {
                        TicketsID = Guid.NewGuid().ToString(),
                        OrderID = orderID,
                        SessionID = request.SessionID,
                        TicketsAreaID = request.TicketsAreaID,
                        RowIndex = int.Parse(parts[0]),
                        SeatIndex = int.Parse(parts[1]),
                        TicketsStatusID = "P",
                        CreatedTime = DateTime.Now
                    });
                }

                // 🚩 唯一存檔點 (觸發 Trigger & Index)
                await _context.SaveChangesAsync();

                if (transaction != null) await transaction.CommitAsync();

                return TimeResponse(bestSeatIDs, (area.Price * request.Count), orderID);
            }
            catch (DbUpdateException)
            {
                if (transaction != null) await transaction.RollbackAsync();
                return new VMBookingResponse { Success = false, Message = "【搶票失敗】座位剛剛被搶先訂走了，請重新嘗試。" };
            }
            catch (Exception ex)
            {
                if (transaction != null) await transaction.RollbackAsync();
                return new VMBookingResponse { Success = false, Message = "系統異常：" + ex.Message };
            }
        }

        // 私有配位邏輯
        private async Task<List<string>> GetBestSeatsAsync(string sessionId, string areaId, int count)
        {
            var area = await _context.TicketsArea.FindAsync(areaId);
            if (area == null || area.TicketsAreaStatusID == "B") return new List<string>();

            var soldTickets = await _context.Tickets
                .Where(t => t.SessionID == sessionId && t.TicketsAreaID == areaId)
                .ToListAsync();

            var allSeats = SeatHelper.GenerateSeatLayout(area.RowCount, area.SeatCount, soldTickets, area.TicketsAreaStatusID);
            return SeatHelper.FindBestSeats(allSeats, count);
        }

        private VMBookingResponse TimeResponse(List<string> seatIDs, decimal totalAmount, string orderID)
        {
            int holdMinutes = 10;
            return new VMBookingResponse
            {
                Success = true,
                Message = "訂單建立成功",
                OrderID = orderID,
                Seats = seatIDs.Select(s => s.Replace("-", "排") + "號").ToList(),
                FinalAmount = totalAmount,
                RemainingSeconds = holdMinutes * 60,
                ExpireTimeText = DateTime.Now.AddMinutes(holdMinutes).ToString("HH:mm:ss")
            };
        }
    }
}