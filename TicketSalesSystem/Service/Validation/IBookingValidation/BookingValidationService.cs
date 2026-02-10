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
        //待改寫活動
        //private async Task<bool> CheckVenueStatus(string VenueID)
        //{
        //    var venue = await _context.Venue.FindAsync(VenueID);
        //    if (venue == null || venue.VenueStatusID != "A")
        //    {
        //        return false;
        //    }
        //    return true;
        //}

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

       public async Task<(bool IsValid, string Message)> ValidateAllAsync(VMBookingRequest request) 
        {
            //if (string.IsNullOrEmpty(request.TicketsAreaID))
            //{
            //    return (false, $"偵錯：後端收到的 TicketsAreaID 是空的！目前的 Request 物件裡只抓到 SessionID: {request.SessionID}");
            //}

            if (!await CheckVenueStatus(request.VenueID)) return (false, "區域暫時不開放");
            if (!await CheckSessionStatus(request.SessionID)) return (false, "場次非售票時間");
            if (!await CheckTicketsAreaStatus(request.TicketsAreaID)) return (false, "票區不可售");

            return (true, "");
        }

    }
}
