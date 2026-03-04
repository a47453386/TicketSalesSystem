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

            // 3. 開始交易與原子扣減
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var rowAffected = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE TicketsArea SET Remaining = Remaining - {0} " +
                    "WHERE TicketsAreaID = {1} AND Remaining >= {2}",
                    request.Count, request.TicketsAreaID, request.Count
                );

                if (rowAffected == 0)
                {
                    await transaction.RollbackAsync();
                    _queueService.ReleaseQueueSlot();
                    return new BookingResultDTO { Success = false, Message = "抱歉，票券已售完！" };
                }

                // 4. 建立訂單 (呼叫你原本的 SeatService)
                var response = await _seatService.CreateOrderAndTicketsAsync(request, memberID);

                if (response.Success)
                {
                    await transaction.CommitAsync();
                    return new BookingResultDTO { Success = true, Data = response };
                }
                else
                {
                    await transaction.RollbackAsync();
                    _queueService.ReleaseQueueSlot();
                    return new BookingResultDTO { Success = false, Message = response.Message };
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _queueService.ReleaseQueueSlot();
                return new BookingResultDTO { Success = false, Message = "系統異常：" + (ex.InnerException?.Message ?? ex.Message) };
            }
        }
    }
}
