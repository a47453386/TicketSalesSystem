[HttpPost]
public async Task<IActionResult> ConfirmBooking([FromBody] BookingRequest request)
{
    // 1. 修復 TotalAmount 紅線：先查單價
    var area = await _context.TicketsArea.FindAsync(request.AreaID);
    if (area == null) return Json(new { success = false, message = "票區不存在" });
    
    decimal totalAmount = area.Price * request.SelectedSeats.Count;

    // 2. 呼叫你的 SQL Function 取得新編號
    // 注意：SqlQueryRaw 在 EF Core 7+ 建議用法如下
    var newOrderId = _context.Database.SqlQueryRaw<string>("SELECT dbo.funGetOrderID()")
                              .AsEnumerable()
                              .FirstOrDefault();

    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // 3. 在 VS 建立 Order
        var order = new Order {
            OrderID = newOrderId,
            MemberID = request.MemberID,
            SessionID = request.SessionID,
            OrderDate = DateTime.Now,
            TotalAmount = totalAmount,
            OrderStatusID = "Pending"
        };
        _context.Order.Add(order);

        // 4. 修復 Split 紅線：對單一字串 seatId 進行處理
        foreach (var seatId in request.SelectedSeats)
        {
            var parts = seatId.Split('-'); // 這裡不會紅線了，因為 seatId 是字串
            if (parts.Length == 2)
            {
                _context.Tickets.Add(new Tickets {
                    TicketsID = Guid.NewGuid().ToString(),
                    OrderID = newOrderId,
                    SessionID = request.SessionID,
                    TicketsAreaID = request.AreaID,
                    RowIndex = int.Parse(parts[0]),
                    SeatIndex = int.Parse(parts[1]),
                    TicketsStatusID = "Sold"
                });
            }
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return Json(new { success = true, orderId = newOrderId });
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return Json(new { success = false, message = ex.Message });
    }
}