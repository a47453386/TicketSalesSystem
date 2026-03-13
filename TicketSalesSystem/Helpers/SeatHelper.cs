using Azure.Core;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel;

namespace TicketSalesSystem.Helpers
{


    public static class SeatHelper
    {
        // 定義清楚可售狀態代碼
        public const string AvailableStatus = "N";

        public static List<VMSeats> GenerateSeatLayout(int rowCount, int seatCount, List<Tickets> soldTickets, string ticketsAreaStatus)
        {
            // 🚩 修正：使用 GroupBy 避免 Key 重複導致 Dictionary 崩潰
            var activeTickets = soldTickets
                .Where(t => t.TicketsStatusID == "S" || t.TicketsStatusID == "P" || t.TicketsStatusID == "Y")
                .GroupBy(t => $"{t.RowIndex}-{t.SeatIndex}")
                .ToDictionary(g => g.Key, g => g.First().TicketsStatusID);

            var seatList = new List<VMSeats>();
            for (int r = 1; r <= rowCount; r++)
            {
                for (int s = 1; s <= seatCount; s++)
                {
                    var seatid = $"{r}-{s}";
                    string currentStatus = AvailableStatus;

                    if (activeTickets.TryGetValue(seatid, out string statusId))
                    {
                        currentStatus = statusId;
                    }

                    seatList.Add(new VMSeats
                    {
                        SeatID = seatid,
                        RowIndex = r,
                        SeatIndex = s,
                        Label = $"{r}排{s}號",
                        Status = currentStatus,
                    });
                }
            }
            return seatList;
        }

        public static List<string> FindBestSeats(List<VMSeats> allSeats, int count)
        {
            // 🚩 優化：將符合條件的排數抓出來，隨機打亂順序
            // 這樣多人同時買票時，系統就不會全部擠在第一排搶位子
            var rowGroups = allSeats
                .Where(s => s.Status == AvailableStatus)
                .GroupBy(s => s.RowIndex)
                .Select(g => new { RowIndex = g.Key, Seats = g.OrderBy(s => s.SeatIndex).ToList() })
                .OrderBy(x => Guid.NewGuid()) // 🚩 關鍵：隨機打亂配位順序，大幅減少撞車機率
                .ToList();

            foreach (var group in rowGroups)
            {
                var seatsInRow = group.Seats;
                if (seatsInRow.Count < count) continue;

                for (int i = 0; i <= seatsInRow.Count - count; i++)
                {
                    var potential = seatsInRow.GetRange(i, count);

                    bool isContinuous = true;
                    for (int j = 0; j < potential.Count - 1; j++)
                    {
                        if (potential[j + 1].SeatIndex - potential[j].SeatIndex != 1)
                        {
                            isContinuous = false;
                            break;
                        }
                    }

                    if (isContinuous)
                    {
                        return potential.Select(s => s.SeatID).ToList();
                    }
                }
            }
            return new List<string>();
        }
    }
}