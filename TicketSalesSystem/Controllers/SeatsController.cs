using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TicketSalesSystem.Helpers;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel;

namespace TicketSalesSystem.Controllers
{
    public class SeatsController : Controller

    {
        private readonly TicketsContext _context;

        public SeatsController(TicketsContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> IndexTest()
        {
            var session = await _context.Session.ToListAsync();

            // 預設不抓票區，等使用者選了場次再透過 AJAX 抓，或者直接全部抓出來
            ViewBag.Sessions = session;

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAreasBySession(string sessionId)
        {
            var areas = await _context.TicketsArea
                .Where(a => a.SessionID == sessionId)
                .Select(a => new {
                    a.TicketsAreaID,
                    a.TicketsAreaName,
                    a.Price,
                    a.VenueID
                })
                .ToListAsync();
            return Json(areas);
        }
        //public async Task<IActionResult> Index(string venueID, string sessionID, string areaID)
        //{
        //    // 1. 從資料庫取得場館資訊 (座位數量)
        //    var venue = await _context.Venue.FirstOrDefaultAsync(v => v.VenueID == venueID);
        //    if (venue == null)
        //    {
        //        return NotFound("找不到場地配置");
        //    }

        //    //2. 取得該場次、該票區「已售出」的 Tickets 列表
        //    var soldTickets = await _context.Tickets
        //        .Where(t => t.SessionID == sessionID && t.TicketsAreaID == areaID)
        //        .ToListAsync();

        //    // 3.呼叫 Helper 產生完整的座位清單 (Model)
        //    var seatList = SeatHelper.GenerateSeatLayout(venue.RowCount, venue.SeatCount, soldTickets);


        //    ViewBag.VenueID = venueID;
        //    ViewBag.SessionID = sessionID;
        //    ViewBag.AreaID = areaID;

        //    // 4. 將清單傳給 Index.cshtml
        //    return View(seatList);
        //}

        //[HttpPost]
        //public async Task<IActionResult> AutoAssign([FromBody] AssignRequest request)
        //{
        //    // 1. 取得場館資訊
        //    var venue = await _context.Venue.FindAsync(request.VenueID);
        //    if (venue == null)
        //    {
        //        return Json(new { success = false, message = "找不到場地配置" });
        //    }
        //    //2. 取得該場次已售出的票
        //    var soldTickets = await _context.Tickets.Where(t => t.SessionID == request.SessionID && t.TicketsAreaID == request.AreaID).ToListAsync();

        //    // 2. 產出當前所有座位狀態
        //    var allSeats = SeatHelper.GenerateSeatLayout(venue.RowCount, venue.SeatCount, soldTickets);

        //    // 3. 找出連續座位
        //    var selectedIDs = SeatHelper.FindBestSeats(allSeats, request.Count);

        //    if (!selectedIDs.Any())
        //    {
        //        return Json(new { success = false, message = "無法找到足夠的連續座位" });
        //    }


        //    return Json(new { success = true, seats = selectedIDs });

        //}


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
                        MemberID = "008783c7-31da-4d39-bba4-70ceed5939e4",
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
