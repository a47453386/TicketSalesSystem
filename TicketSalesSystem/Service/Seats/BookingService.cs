using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TicketSalesSystem.DTOs;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.ID;
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.Service.Orders;
using TicketSalesSystem.Service.Queue;
using TicketSalesSystem.Service.Validation.NewFolder;
using TicketSalesSystem.ViewModel.Booking;

namespace TicketSalesSystem.Service.Seats
{
    public class BookingService
    {
        private readonly ISeatService _seatService;        
        private readonly TicketsContext _context;
        private readonly IQueueService _queueService;

        public BookingService(ISeatService seatService, IBookingValidationService bookingValidation,
            TicketsContext context, IOrderService orderService, IMemoryCache memoryCache,
            IQueueService queueService, IUserAccessorService userAccessorService)
        {
            _seatService = seatService;            
            _context = context;
            _queueService = queueService;
        }

        public async Task<BookingResultDTO> ConfirmBookingAsync(VMBookingRequest request, string memberID)
        {
            try
            {
                // 1. 僅做初步庫存檢查 (不扣除，只看夠不夠)
                var area = await _context.TicketsArea.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.TicketsAreaID == request.TicketsAreaID);

                if (area == null || area.Remaining < request.Count)
                {
                    _queueService.ReleaseQueueSlot();
                    return new BookingResultDTO { Success = false, Message = "抱歉，票券已售完！" };
                }

                // 2. 🚩 呼叫 SeatService 執行核心交易
                // 交易與防超賣邏輯都鎖在 CreateOrderAndTicketsAsync 內部
                var response = await _seatService.CreateOrderAndTicketsAsync(request, memberID);

                if (!response.Success)
                {
                    _queueService.ReleaseQueueSlot();
                }

                return new BookingResultDTO
                {
                    Success = response.Success,
                    Data = response,
                    Message = response.Message
                };
            }
            catch (Exception ex)
            {
                _queueService.ReleaseQueueSlot();
                return new BookingResultDTO { Success = false, Message = "系統異常：" + ex.Message };
            }
        }
    }
}
