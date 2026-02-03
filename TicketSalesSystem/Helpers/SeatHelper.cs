using Azure.Core;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel;

namespace TicketSalesSystem.Helpers
{

    //產生座位表
    public static class SeatHelper
    {
        public static List<VMSeats> GenerateSeatLayout(int rowCount, int seatCount, List<Tickets> soldTickets)
        {
            var activeTickets = soldTickets
                .Where(t => t.TicketsStatusID == "S" || t.TicketsStatusID == "P" || t.CreatedTime.AddMinutes(10) > DateTime.Now)
                .ToList();

            var seatList = new List<VMSeats>();
            var soldSet = new HashSet<string>(
                soldTickets.Select(t => $"{t.RowIndex}-{t.SeatIndex}")
                );
            for (int r = 1; r <= rowCount; r++)
            {
                for (int s = 1; s <= seatCount; s++)
                {
                    var seatid = $"{r}-{s}";
                    var label = $"{r}排{s}號";

                    seatList.Add(new VMSeats
                    {
                        SeatID = seatid,
                        RowIndex = r,
                        SeatIndex = s,
                        Label = label,
                        Status = soldSet.Contains(seatid) ? "已售" : "可售"
                    });
                }
            }
            return seatList;
        }


        // 自動配位演算法：尋找連續座位
        public static List<string> FindBestSeats(List<VMSeats> allSeats, int count)
        {

            var rowGroups = allSeats
                .Where(s => s.Status == "可售")
                .GroupBy(s => s.RowIndex)
                .OrderBy(g => g.Key);// 依照排數排序

            foreach (var row in rowGroups)
            {
                var seatsInRow = row.OrderBy(s => s.SeatIndex).ToList();
                if (seatsInRow.Count < count)
                {
                    continue; // 如果該排可售座位數量不足，跳過
                }

                for (int i = 0; i <= seatsInRow.Count - count; i++)
                {
                    var potential = seatsInRow.GetRange(i, count);
                    // 核心修改：嚴格檢查 SeatIndex 是否連續
                    // 防止例如 [1, 2, 5] 這種雖然有 3 張但中間斷掉的情況
                    bool isContinuous = true;
                    // 檢查是否真的連續（序號差值檢查）
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
            return new List<string>(); // 找不到則回傳空清單

        }

        //取得最佳連續座位
       public static List<string> GetBestAvailableSeats(int rows, int cols, List<Tickets> soldTickets, int count)
        {
            // 呼叫你的 Helper 產生佈局 (假設你的票區固定是 10x10)
            // 如果你有不同的區有不同排數，可以在這裡寫判斷
            var allSeats = GenerateSeatLayout(rows, cols, soldTickets);

            // 呼叫你的 Helper 演算法找「最佳連號座位」
            var bestSeatIDs = FindBestSeats(allSeats, count);

            return bestSeatIDs?? new List<string>();

        }



    }
}