using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.Validation.NewFolder;
using TicketSalesSystem.ViewModel.Booking;

namespace TicketSalesSystem.Service.Validation.IBookingValidation
{
    public class BookingValidationService: IBookingValidationService
    {
        private readonly TicketsContext _context;
        public BookingValidationService(TicketsContext context)
        {
            _context = context;
        }
        

        //Venue
        private async Task<bool> CheckVenueStatus(string VenueID)
        {
            var venue = await _context.Venue.FindAsync(VenueID);
            if (venue == null || venue.VenueStatusID != "A")
            {
                return false; 
            }
            return true;
        }

        //Session
        private async Task<bool> CheckSessionStatus(string SessionID) 
        {
            var session = await _context.Session.FindAsync(SessionID);
            if (session == null || DateTime.Now < session.SaleStartTime||DateTime.Now> session.SaleEndTime)
            {
                return false;
            }
            return true;
        }

        //TicketsArea
        private async Task<bool> CheckTicketsAreaStatus(string TicketsAreaID) 
        {
            var ticketsArea = await _context.TicketsArea.FindAsync(TicketsAreaID);
            if (ticketsArea == null || ticketsArea.TicketsAreaStatusID!="A")
            {
                return false;
            }
            return true;
        }

        // 在 BookingValidationService 補一個簡單的庫存檢查
        private async Task<bool> CheckInitialStock(string TicketsAreaID, int count)
        {
            var area = await _context.TicketsArea
                .AsNoTracking() // 這裡用不追蹤查詢，速度最快
                .FirstOrDefaultAsync(a => a.TicketsAreaID == TicketsAreaID);

            // 如果這裡就發現剩餘票數 < 購買張數，直接回傳失敗，不用去跑後面的 Transaction
            return area != null && area.Remaining >= count;
        }

        private async Task<(bool Success, string Message)> CheckProgrammePurchaseLimit(string memberId, string sessionId, int requestCount)
        {
            // 1. 先找出這個場次所屬的「活動 ID (ProgrammeID)」
            var programmeData = await _context.Session
                 .Where(s => s.SessionID == sessionId)
                 .Select(s => new {
                     s.ProgrammeID,
                     s.Programme.LimitPerOrder 
                 })
                 .FirstOrDefaultAsync();
            

            // 2. 統計該會員在「該活動」下所有場次已持有的有效票數
            // 邏輯：Tickets -> Session -> ProgrammeID 必須匹配，且訂單不能是已取消 (N)與待付款(P)
            var activeStatuses = new[] { "P", "S" };
            int existingTicketsCount = await _context.Tickets
                .Include(t => t.Order)
                .Include(t => t.Session)
                .Where(t => t.Order.MemberID == memberId &&
                            t.Session.ProgrammeID == programmeData.ProgrammeID &&
                            activeStatuses.Contains(t.Order.OrderStatusID)) // N 為已失效/已取消
                .CountAsync();

            // 3. 判斷是否超過上限
            if ((existingTicketsCount + requestCount) > programmeData.LimitPerOrder)
            {
                return (false, $"本活動每人限購 {programmeData.LimitPerOrder} 張，您目前已持有 {existingTicketsCount} 張，本次申請 {requestCount} 張，已超過上限。");
            }

            return (true, "");

        }



        public async Task<(bool IsValid, string Message)> ValidateAllAsync(VMBookingRequest request, string memberId) 
        {
            //if (string.IsNullOrEmpty(request.TicketsAreaID))
            //{
            //    return (false, $"偵錯：後端收到的 TicketsAreaID 是空的！目前的 Request 物件裡只抓到 SessionID: {request.SessionID}");
            //}

            if (!await CheckVenueStatus(request.VenueID)) return (false, "區域暫時不開放");
            if (!await CheckSessionStatus(request.SessionID)) return (false, "場次非售票時間");
            if (!await CheckTicketsAreaStatus(request.TicketsAreaID)) return (false, "票區不可售");
            if (!await CheckInitialStock(request.TicketsAreaID, request.Count)) return (false, "庫存不足");

            var limitResult = await CheckProgrammePurchaseLimit(memberId, request.SessionID, request.Count);
            if (!limitResult.Success)
            {
                return (false, limitResult.Message);
            }

            return (true, "");
        }

    }
}
