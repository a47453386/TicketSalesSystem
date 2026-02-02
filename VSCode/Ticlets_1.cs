[HttpPost]
public async Task<IActionResult> ConfirmBooking([FromBody] BookingRequest request)
{
    // 1. 驗證票區並抓取票價 (避免前端竄改金額)
    var area = await _context.TicketsArea.FindAsync(request.AreaID);
    if (area == null) return Json(new { success = false, message = "找不到票區資訊" });

    // 2. 取得自定義訂單 ID (由預存程序或函數產生)
    var newOrderID = _context.Database.SqlQueryRaw<string>("SELECT dbo.funGetOrderID() ").AsEnumerable().FirstOrDefault();
    if (string.IsNullOrEmpty(newOrderID)) newOrderID = Guid.NewGuid().ToString().Substring(0, 10); // 備用方案

    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // 3. 建立訂單
        var order = new Order
        {
            OrderID = newOrderID,
            OrderCreatedTime = DateTime.Now,
            MemberID = request.MemberID,
            SessionID = request.SessionID,
            PaymentMethodID = request.PaymentMethodID,
            OrderStatusID = "Y", 
            PaymentTradeNO = Guid.NewGuid().ToString().Substring(0, 12) // 模擬交易序號
        };
        _context.Order.Add(order); // 請確認你的 Context 是 .Order 還是 .Orders

        // 4. 建立票券明細並轉換座位名稱
        var ticketList = new List<object>();
        foreach (var seatID in request.Seats)
        {
            var parts = seatID.Split('-');
            if (parts.Length == 2)
            {
                int r = int.Parse(parts[0]);
                int s = int.Parse(parts[1]);
                string seatDisplayName = $"{(char)('A' + r)}排 {s + 1}號";

                var ticket = new Tickets
                {
                    TicketsID = Guid.NewGuid().ToString(),
                    RowIndex = r,
                    SeatIndex = s,
                    CreatedTime = DateTime.Now,
                    OrderID = newOrderID,
                    SessionID = request.SessionID,
                    TicketsAreaID = request.AreaID,
                    TicketsStatusID = "Sold"
                };
                _context.Tickets.Add(ticket);
                ticketList.Add(new { Seat = seatDisplayName, Price = area.Price });
            }
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        // 5. 回傳明細給前端
        return Json(new { 
            success = true, 
            message = "訂票成功！", 
            orderID = newOrderID,
            details = new {
                AreaName = area.AreaName, // 假設欄位名
                Total = area.Price * request.Seats.Count,
                Items = ticketList
            }
        });
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        // 這裡會抓到具體錯誤，例如：外鍵約束失效
        return Json(new { success = false, message = "訂購失敗：" + (ex.InnerException?.Message ?? ex.Message) });
    }
}