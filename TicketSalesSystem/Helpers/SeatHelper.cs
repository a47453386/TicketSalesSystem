using Azure.Core;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel;

namespace TicketSalesSystem.Helpers
{

   
    public static class SeatHelper
    {
        static string ticketStatusCode = "N";

        //產生座位表
        public static List<VMSeats> GenerateSeatLayout(int rowCount, int seatCount, List<Tickets> soldTickets,string ticketsAreaStatus)
        {
            //篩選出有效的已售票券 (已售出或佔位中，或是10分鐘內剛建立的)
            var activeTickets = soldTickets
                .Where(t => t.TicketsStatusID == "S" || t.TicketsStatusID == "P" || t.CreatedTime.AddMinutes(10) > DateTime.Now)
                .ToList();


            //var soldSet = new HashSet<string>(
            //    soldTickets.Select(t => $"{t.RowIndex}-{t.SeatIndex}")
            //    );
            //準備好「有人坐」的名單，此段我試著加，但失敗也沒關係
            var soldDict = new Dictionary<string, string>();
            foreach (var t in activeTickets)
            {
                string key = $"{t.RowIndex}-{t.SeatIndex}";
                if (!soldDict.ContainsKey(key))
                {
                    soldDict.Add(key, t.TicketsStatusID);
                }
            }

            var seatList = new List<VMSeats>();

            //開始畫所有的位子 (rowCount * seatCount)
            for (int r = 1; r <= rowCount; r++)
            {
                for (int s = 1; s <= seatCount; s++)
                {
                    var seatid = $"{r}-{s}";
                    var label = $"{r}排{s}號";

                    //比對名單，決定該格座位的狀態
                    string statusText = "可售";                    
                    if (soldDict.TryGetValue(seatid, out string statusId))//out string statusId嘗試抓取值，如果抓到了就順便把它存進這個變數裡
                    {
                        var ticketStatusCode = statusId;
                        statusText = (ticketStatusCode == "S") ? "已售" : "保留中";
                    }

                    seatList.Add(new VMSeats
                    {
                        SeatID = seatid,
                        RowIndex = r,
                        SeatIndex = s,
                        Label = label,
                        Status = ticketStatusCode,
                    });
                }
            }
            return seatList;
        }


        // 自動配位演算法：尋找連續座位
        public static List<string> FindBestSeats(List<VMSeats> allSeats, int count)
        {

            var rowGroups = allSeats
                .Where(s => s.Status == ticketStatusCode)
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

       

        

    }
}