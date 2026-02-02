[HttpPost]
public async Task<IActionResult> ConfirmBooking([FromBody] BookingRequest request)
{
    // 1. 抓取已售票券，用來產生目前的座位狀態
    var soldTickets = await _context.Tickets
        .Where(t => t.SessionID == request.SessionID && t.TicketsAreaID == request.AreaID)
        .ToListAsync();

    // 2. 呼叫你的 Helper 產生佈局 (假設你的票區固定是 10x10)
    // 如果你有不同的區有不同排數，可以在這裡寫判斷
    var allSeats = SeatHelper.GenerateSeatLayout(10, 10, soldTickets);

    // 3. 呼叫你的 Helper 演算法找「最佳連號座位」
    var bestSeatIDs = SeatHelper.FindBestSeats(allSeats, request.TicketCount);

    if (!bestSeatIDs.Any())
    {
        return Json(new { success = false, message = "抱歉，找不到足夠的連續座位，請嘗試減少張數" });
    }

    // 4. 取得自動編號
    var newOrderID = _context.Database.SqlQueryRaw<string>("SELECT dbo.funGetOrderID()")
                              .AsEnumerable().FirstOrDefault();

    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // 5. 建立訂單
        var order = new Order {
            OrderID = newOrderID,
            MemberID = request.MemberID,
            SessionID = request.SessionID,
            OrderCreatedTime = DateTime.Now,
            OrderStatusID = "Y", // 注意：SQL 長度要夠
            PaymentMethodID = request.PaymentMethodID, // 注意：SQL 長度要夠
            PaymentTradeNO = Guid.NewGuid().ToString().Substring(0, 10)
        };
        _context.Order.Add(order);

        // 6. 建立票券 (拆解 SeatID 存回座標)
        foreach (var seatID in bestSeatIDs)
        {
            var parts = seatID.Split('-'); // 你的 SeatID 格式是 "1-5"
            _context.Tickets.Add(new Tickets {
                TicketsID = Guid.NewGuid().ToString(),
                OrderID = newOrderID,
                SessionID = request.SessionID,
                TicketsAreaID = request.AreaID,
                RowIndex = int.Parse(parts[0]),
                SeatIndex = int.Parse(parts[1]),
                TicketsStatusID = "Sold", // 注意：SQL 長度要夠
                CreatedTime = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Json(new { success = true, orderID = newOrderID, seats = bestSeatIDs });
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return Json(new { success = false, message = "訂購失敗：" + ex.Message });
    }
}