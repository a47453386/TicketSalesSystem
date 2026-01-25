using Microsoft.EntityFrameworkCore;

namespace TicketSalesSystem.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (TicketsContext context = new TicketsContext(serviceProvider.GetRequiredService<DbContextOptions<TicketsContext>>()))
            {
                //寫一張，如有找到就不執行
                if (context.Member.Any())
                {
                    return;
                }

                //活動
                context.Programme.AddRange(
                    new Programme()
                    {
                        ProgrammeID = "20260101",
                        ProgrammeName = "五月天 2026 世界巡迴演唱會",
                        ProgrammeDescription = "五月天 2026 世界巡迴演唱會－台北站，經典曲目全新編排。",
                        CreatedTime = new DateTime(2026, 1, 1),
                        UpdatedAt = null,
                        CoverImage = "C20260101.jpg",
                        SeatImage = "S20260101.jpg",
                        LimitPerOrder = 4
                    },
                    new Programme
                    {
                        ProgrammeID = "20260201",
                        ProgrammeName = "周杰倫 嘉年華 世界巡演",
                        ProgrammeDescription = "周杰倫嘉年華演唱會，橫跨經典與新作。",
                        CreatedTime = new DateTime(2026, 2, 1),
                        UpdatedAt = null,
                        CoverImage = "C20260201.jpg",
                        SeatImage = "S20260201.jpg",
                        LimitPerOrder = 6
                    },
                    new Programme
                    {
                        ProgrammeID = "20260301",
                        ProgrammeName = "音樂劇《悲慘世界》",
                        ProgrammeDescription = "經典音樂劇《Les Misérables》中文版。",
                        CreatedTime = new DateTime(2026, 2, 1),
                        UpdatedAt = null,
                        CoverImage = "C20260301.jpg",
                        SeatImage = "S20260301.jpg",
                        LimitPerOrder = 2
                    }
                );
                context.SaveChanges();
                context.Session.AddRange(
                    new Session
                    {
                        SessionID = "2026010101",
                        SaleStartTime = new DateTime(2025, 12, 1, 12, 0, 0),
                        SaleEndTime = new DateTime(2026, 1, 9, 23, 59, 59),
                        StartTime = new DateTime(2026, 1, 10, 19, 30, 0),
                        ProgrammeID = "20260101"
                    },
                    new Session
                    {
                        SessionID = "2026020101",
                        SaleStartTime = new DateTime(2025, 12, 2, 12, 0, 0),
                        SaleEndTime = new DateTime(2026, 1, 10, 23, 59, 59),
                        StartTime = new DateTime(2026, 1, 11, 19, 30, 0),
                        ProgrammeID = "20260201"
                    },
                    new Session
                    {
                        SessionID = "2026030101",
                        SaleStartTime = new DateTime(2025, 12, 3, 12, 0, 0),
                        SaleEndTime = new DateTime(2026, 1, 11, 23, 59, 59),
                        StartTime = new DateTime(2026, 1, 12, 19, 30, 0),
                        ProgrammeID = "20260301"
                    }
                    );
                context.SaveChanges();
            }
        }
    }
}
