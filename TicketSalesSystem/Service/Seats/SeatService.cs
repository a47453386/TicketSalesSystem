using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
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

        //取得場次列表
        public Task<List<Session>> GetSessionsAsync()
        {
            var session =  _context.Session.ToListAsync();

            return session;
        }


        //同步票區狀態
        public async Task SyncAreaStatusAaync(string areaId)
        {
            //檢查區域是否有"N"(可售的位子)
            bool hasAvailableSeats = await _context.Tickets
               .AnyAsync(t => t.TicketsAreaID == areaId && t.TicketsStatusID == "N");

            //取得該區域的票區資料
            var area = await _context.TicketsArea.FindAsync(areaId);
            if (area == null)
            {
                return;
            }

            //根據是否有可售位子更新票區狀態
            //如果有可售位子，且票區狀態為完售("O")，將票區改成售票中("I")
            if (hasAvailableSeats && area.TicketsAreaID == "O")
            {
                area.TicketsAreaID = "I";
            }
            //沒有可售位子，且票區狀態為售票中("I")，將票區改成完售("O")
            if (!hasAvailableSeats && area.TicketsAreaID == "I")
            {
                area.TicketsAreaID = "O";
            }
            //將狀態儲存到資料庫
            await _context.SaveChangesAsync();
        }

        //取得特定票區的座位
        public async Task<List<VMSeats>> GetAreaLayoutAsync(string areaId)
        {
            var area = await _context.TicketsArea.FindAsync(areaId);
            if (area == null)
            {
                return null;
            }

            var soldTickets = await _context.Tickets
                .Where(t => t.TicketsAreaID == areaId && t.TicketsStatusID != "N")
                .ToListAsync();

            var layout = SeatHelper.GenerateSeatLayout(area.RowCount, area.SeatCount, soldTickets, area.TicketsAreaStatusID);

            return layout;

        }

        //場次的所有票區
        public async Task<IEnumerable<object>> GetAreasBySession(string sessionId)
        {
            var areas = await _context.TicketsArea
                .Where(a => a.SessionID == sessionId)
                .Select(a => new
                {
                    a.TicketsAreaID,
                    a.TicketsAreaName,
                    a.RowCount,
                    a.SeatCount,
                    a.Price,
                    a.VenueID,
                    a.TicketsAreaStatusID
                })
                .ToListAsync();

            return areas;
        }

        //總金額計算
        private decimal CalculateTotalAmount(decimal price, int Count)
        {
            return price * Count;
        }

        //取得最佳連續座位
        private async Task<List<string>> GetBestSeatsAsync(string sessionId, string areaId, int count)
        {
            //取得票區資料
            var area = await _context.TicketsArea.FindAsync(areaId);
            if (area == null || area.TicketsAreaStatusID == "B")
            {
                return new List<string>();
            }
            //取得該場次、該票區「可售(N)」的所有票券
            var soldTickets = await _context.Tickets
                .Where(t => t.SessionID == sessionId && t.TicketsAreaID == areaId && t.TicketsStatusID != "N")
                .ToListAsync();

            //呼叫 Helper 產生座位表
            var allSeats = SeatHelper.GenerateSeatLayout(area.RowCount, area.SeatCount, soldTickets, area.TicketsAreaStatusID);
            var bestSeatIDs = SeatHelper.FindBestSeats(allSeats, count);
            if (bestSeatIDs == null)
            {
                return new List<string>();
            }
            //可以寫成bestSeatIDs ?? new List<string>();
            return bestSeatIDs;
        }

        //保留時間
        private  VMBookingResponse TimeResponse(List<string> seatIDs, decimal totalAmount, string orderID)
        {
            int holdMinutes = 10;

            var response = new VMBookingResponse
            {
                Success = true,
                Message = "訂單建立成功",
                OrderID= orderID,
                Seats = seatIDs.Select(s => s.Replace("-", "排") + "號").ToList(),
                FinalAmount = totalAmount,
                RemainingSeconds = holdMinutes * 60,
                ExpireTimeText = DateTime.Now.AddMinutes(holdMinutes).ToString("HH:mm:ss")
            };

            return response;

        }
        
                    


        //建立訂單與票券
        public async Task<VMBookingResponse> CreateOrderAndTicketsAsync(VMBookingRequest request, string memberID)
        {
            //驗證區域狀態
            var area = await _context.TicketsArea.FindAsync(request.TicketsAreaID);
            if (area == null || area.TicketsAreaStatusID != "A")
            {
                return new VMBookingResponse
                {
                    Success = false,
                    Message = $"票區狀態錯誤！資料庫內是 '{area.TicketsAreaStatusID}'，但程式要求必須是 'A'。"
                };
            }

            //取得座位
            var bestSeatIDs = await GetBestSeatsAsync(area.SessionID, area.TicketsAreaID, request.Count);
            if (!bestSeatIDs.Any())
            {
                return new VMBookingResponse
                {
                    Success = false,
                    Message = "剩餘連續座位不足"
                };
            }

            //取得自動編號，.AsEnumerable()是「我不讓EF翻SQL 了，接下來我自己用 C# 處理」
            var newOrderID = _context.Database.SqlQueryRaw<string>("SELECT dbo.funGetOrderID()")
                             .AsEnumerable().FirstOrDefault();

            //計算總金額
            decimal Amount = CalculateTotalAmount(area.Price, request.Count);


            //執行資料庫交易
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {                                       
                    //建立訂單
                    var order = new Order
                    {
                        OrderID = newOrderID,
                        OrderCreatedTime = DateTime.Now,
                        PaymentTradeNO = Guid.NewGuid().ToString(),
                        PaymentDescription = null,
                        PaymentStatus = false,
                        MemberID = memberID,
                        SessionID = request.SessionID,
                        OrderStatusID = "P", // 注意：SQL 長度要夠
                        PaymentMethodID = "A", // 注意：SQL 長度要夠
                        PaidTime = null
                    };
                    _context.Order.Add(order);

                    //建立票券 (拆解 SeatID 存回座標)
                    foreach (var seatID in bestSeatIDs)
                    {
                        var parts = seatID.Split('-'); // 你的 SeatID 格式是 "1-5"
                        _context.Tickets.Add(new Tickets
                        {
                            TicketsID = Guid.NewGuid().ToString(),
                            OrderID = newOrderID,
                            SessionID = request.SessionID,
                            TicketsAreaID = request.TicketsAreaID,
                            RowIndex = int.Parse(parts[0]),
                            SeatIndex = int.Parse(parts[1]),
                            TicketsStatusID = "P", // 注意：SQL 長度要夠
                            CreatedTime = DateTime.Now,
                            ScannedTime = null,
                            RefundTime = null
                        });
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();


                    //更新票區狀態
                    await SyncAreaStatusAaync(request.TicketsAreaID);

                    //保留時間
                    return TimeResponse(bestSeatIDs, Amount,newOrderID);


                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }


        

    }

}
