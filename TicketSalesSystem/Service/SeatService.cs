using Azure.Core;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Helpers;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel;

namespace TicketSalesSystem.Service
{
    public class SeatService : ISeatService
    {
        private readonly TicketsContext _context;
        public SeatService(TicketsContext context)
        {
            _context = context;
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

        //取得特定票區的座位區
        public async Task<List<VMSeats>> GetAreaLayoutAsync(string areaId)
        {
            var area = await _context.TicketsArea.FindAsync(areaId);
            if (area == null)
            {
                return null;
            }

            var soldTickets = await _context.Tickets
                .Where(t => t.TicketsAreaID == areaId && t.TicketsStatusID == "N")
                .ToListAsync();

            var layout = SeatHelper.GenerateSeatLayout(area.RowCount, area.SeatCount, soldTickets, area.TicketsAreaStatusID);

            return layout;

        }

    }
}

