using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TicketSalesSystem.Helpers;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service;
using TicketSalesSystem.ViewModel;

namespace TicketSalesSystem.Controllers
{
    public class SeatsController : Controller

    {
        private readonly ISeatService _context;

        public SeatsController(ISeatService seatService)
        {
            _context = seatService;
        }
        public async Task<IActionResult> IndexTest(string id)
        {
            var session = await _context.GetAreaLayoutAsync(id);

            // 預設不抓票區，等使用者選了場次再透過 AJAX 抓，或者直接全部抓出來
            ViewBag.Sessions = session;

            return View();
        }
        
        public async Task<IActionResult> GetAreasBySession(string sessionId)
        {
            var areas = await _context.GetAreaLayoutAsync(sessionId);

            return Json(areas);
        }
       
      

        [HttpPost]
        public async Task<IActionResult> ConfirmBooking([FromBody] VMBookingRequest request)
        {

            //總金額
            var area = await _context.TicketsArea.FindAsync(request.AreaID);
            if (area == null)
            {
                return Json(new { success = false, message = "找不到票區" });
            }
            decimal realTotalAmount = area.Price * request.Count;

            // 取得座位
            //場次票區
            var soldTickets = await _context.Tickets
                .Where(t => t.SessionID == request.SessionID && t.TicketsAreaID == request.AreaID)
                .ToListAsync();            
            var bestSeatIDs = SeatHelper.GetBestAvailableSeats(20,15, soldTickets,request.Count);
            if (!bestSeatIDs.Any())
            {
                return Json(new { success = false, message = "抱歉，找不到足夠的連續座位，請嘗試減少張數" });
            }


            //取得自動編號，.AsEnumerable()是「我不讓 EF 翻 SQL 了，接下來我自己用 C# 處理」
            var newOrderID = _context.Database.SqlQueryRaw<string>("SELECT dbo.funGetOrderID()")
                                      .AsEnumerable().FirstOrDefault();

            //建立訂單
            //BeginTransactionAsync() =「我要把接下來的多個資料庫操作包成一個不可分割的單位」
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    //鎖定座位((UPDLOCK, ROWLOCK) =「我要鎖住這一筆資料，別人暫時不能動」)
                    var newTicket = _context.Tickets
                        .FromSqlRaw("Select * from Tickets with (UPDLOCK,ROWLOCK)  where SessionID={0}  and TicketsAreaID={1} "
                        ,request.SessionID,request.AreaID)
                        .ToList();
                    //重新取得最佳連續座位
                    var newBestSeat = SeatHelper.GetBestAvailableSeats(20, 15, soldTickets, request.Count);
                    if (newBestSeat==null || !newBestSeat.Any())
                    {
                        throw new Exception("抱歉，該區域連續座位已售罄或不足。");
                    }

                    // 5. 建立訂單
                    var order = new Order
                    {
                        OrderID = newOrderID,
                        OrderCreatedTime = DateTime.Now,
                        PaymentTradeNO = Guid.NewGuid().ToString(),
                        PaymentDescription = null,
                        PaymentStatus = false,
                        MemberID = "36e5a04b-8ba2-4996-8179-10ae5d0cd08f",
                        SessionID = request.SessionID,
                        OrderStatusID = "P", // 注意：SQL 長度要夠
                        PaymentMethodID = "A", // 注意：SQL 長度要夠
                        PaidTime = null
                    };
                    _context.Order.Add(order);

                    // 6. 建立票券 (拆解 SeatID 存回座標)
                    foreach (var seatID in bestSeatIDs)
                    {
                        var parts = seatID.Split('-'); // 你的 SeatID 格式是 "1-5"
                        _context.Tickets.Add(new Tickets
                        {
                            TicketsID = Guid.NewGuid().ToString(),
                            OrderID = newOrderID,
                            SessionID = request.SessionID,
                            TicketsAreaID = request.AreaID,
                            RowIndex = int.Parse(parts[0]),
                            SeatIndex = int.Parse(parts[1]),
                            TicketsStatusID = "P", // 注意：SQL 長度要夠
                            CreatedTime = DateTime.Now,
                            ScannedTime = null,
                            RefundTime = null
                        });
                    }

                    //
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    //保留時間
                    int holdMinutes = 10;
                    DateTime expireTime=DateTime.Now.AddMinutes(holdMinutes);


                    var response= new VMBookingResponse
                    {
                        Success = true,
                        Message = "訂單建立成功",
                        OrderID = newOrderID,
                        Seats = bestSeatIDs.Select(s=>s.Replace("-", "排") + "號").ToList(),
                        FinalAmount = realTotalAmount,
                        RemainingSeconds= holdMinutes * 60,
                        ExpireTimeText = expireTime.ToString("HH:mm:ss")
                    };

                    return Json(response);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // 這樣可以在瀏覽器 alert 看到更詳細的錯誤內容
                    var innerMsg = ex.InnerException != null ? ex.InnerException.Message : "";
                    return Json(new { success = false, message = ex.Message + " | " + innerMsg });
                }
            }
                
        }
    
        
        
    }
}
