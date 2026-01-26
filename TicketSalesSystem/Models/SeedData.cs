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
                        LimitPerOrder = 4,
                        EmployeeID = "P23001",
                        PlaceID = "B",
                        ProgrammeStatusID = "B"
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
                        LimitPerOrder = 6,
                        EmployeeID = "P23001",
                        PlaceID = "A",
                        ProgrammeStatusID = "B"
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
                        LimitPerOrder = 2,
                        EmployeeID = "P23001",
                        PlaceID = "A",
                        ProgrammeStatusID = "A"
                    }
                );
                context.SaveChanges();

                //場次
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

                //場地
                context.Place.AddRange(
                    new Place
                    {
                        PlaceID = "A",
                        PlaceName = "台北小巨蛋",
                        PlaceAddress = "台北市松山區南京東路四段2號"
                    },
                    new Place
                    {
                        PlaceID = "B",
                        PlaceName = "高雄巨蛋",
                        PlaceAddress = "高雄市左營區博愛二路757號"
                    }
                    );
                context.SaveChanges();

                //活動狀態
                context.ProgrammeStatus.AddRange(
                    new ProgrammeStatus
                    {
                        ProgrammeStatusID = "A",
                        ProgrammeStatusName = "未上架"
                    },
                    new ProgrammeStatus
                    {
                        ProgrammeStatusID = "B",
                        ProgrammeStatusName = "上架中"
                    }
                    );
                context.SaveChanges();

                //員工
                context.Employee.AddRange(
                     new Employee
                     {
                         EmployeeID = "P23001",
                         Name = "陳信宏",
                         HireDate = new DateTime(2023, 1, 1),
                         Address = "台北市北投區中山路1巷8號",
                         Birthday = new DateTime(1978, 12, 6),
                         Tel = "0912000001",
                         Gender = true,
                         NationalID = "A123456789",
                         Email = "Ashinn@mayday.com",
                         Extension = "#101",
                         Photo = "F23001.jpg",
                         CreatedTime = DateTime.Now,
                         LastLoginTime = null,
                         RoleID = "P",
                         AccountStatusID = "A"
                     },
                     new Employee
                     {
                         EmployeeID = "A23025",
                         Name = "溫尚翊",
                         HireDate = new DateTime(2023, 6, 1),
                         Address = "台北市大安區復興一路5巷9號",
                         Birthday = new DateTime(1979, 12, 20),
                         Tel = "0912000002",
                         Gender = true,
                         NationalID = "B234567890",
                         Email = "Monster@mayday.com",
                         Extension = "#102",
                         Photo = "F23025.jpg",
                         CreatedTime = DateTime.Now,
                         LastLoginTime = null,
                         RoleID = "A1",
                         AccountStatusID = "A"
                     },
                     new Employee
                     {
                         EmployeeID = "F24001",
                         Name = "劉冠佑",
                         HireDate = new DateTime(2024, 1, 10),
                         Address = "新北市板橋區永和路4巷7號",
                         Birthday = new DateTime(1970, 04, 5),
                         Tel = "0912000003",
                         Gender = true,
                         NationalID = "C345678901",
                         Email = "Ming@mayday.com",
                         Extension = "#103",
                         Photo = "F24001.jpg",
                         CreatedTime = DateTime.Now,
                         LastLoginTime = null,
                         RoleID = "F1",
                         AccountStatusID = "A"
                     }

                    );
                context.SaveChanges();

                //員工帳號密碼
                context.EmployeeLogin.AddRange(
                    new EmployeeLogin
                    {
                        Account = "admin0001",
                        Password = "Aa123456",
                        EmployeeID = "P23001"
                    },
                    new EmployeeLogin
                    {
                        Account = "staff0002",
                        Password = "Bb123456",
                        EmployeeID = "A23025"
                    },
                    new EmployeeLogin
                    {
                        Account = "staff0003",
                        Password = "Cc123456",
                        EmployeeID = "F24001"
                    }
                    );
                context.SaveChanges();


                //角色
                context.Role.AddRange(
                    new Role { RoleID = "A", RoleName = "系統管理員" },
                    new Role { RoleID = "P", RoleName = "活動管理員" },
                    new Role { RoleID = "F", RoleName = "財務人員" }
                    );
                context.SaveChanges();

                //帳號狀態
                context.AccountStatus.AddRange(
                     new AccountStatus
                     {
                         AccountStatusID = "A",
                         AccountStatusName = "正常"
                     },
                     new AccountStatus
                     {
                         AccountStatusID = "B",
                         AccountStatusName = "停權"
                     }
                    );
                context.SaveChanges();

                //會員
                string[] guidMember = { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
                context.Member.AddRange(
                    new Member
                    {
                        MemberID = guidMember[0],
                        Name = "王小明",
                        Address = "台北市信義區松仁路100號",
                        Birthday = new DateTime(1990, 5, 15),
                        Tel = "0911111111",
                        Gender = true,
                        NationalID = "K124935117",
                        Email = "member01@test.com",
                        CreatedDate = DateTime.Now,
                        LastLoginTime = null,
                        IsPhoneVerified = true,
                        AccountStatusID = "A"
                    },
                    new Member
                    {
                        MemberID = guidMember[1],
                        Name = "陳小華",
                        Address = "新北市板橋區文化路200號",
                        Birthday = new DateTime(1985, 8, 20),
                        Tel = "0922222222",
                        Gender = false,
                        NationalID = "L203325122",
                        Email = "member02@test.com",
                        CreatedDate = DateTime.Now,
                        LastLoginTime = null,
                        IsPhoneVerified = false,
                        AccountStatusID = "A"
                    },
                    new Member
                    {
                        MemberID = guidMember[2],
                        Name = "林小美",
                        Address = "台中市西屯區台灣大道300號",
                        Birthday = new DateTime(1995, 12, 25),
                        Tel = "0933333333",
                        Gender = false,
                        NationalID = "T216691249",
                        Email = "member03@test.com",
                        CreatedDate = DateTime.Now,
                        LastLoginTime = null,
                        IsPhoneVerified = false,
                        AccountStatusID = "B"
                    }
                    );
                context.SaveChanges();

                //會員登入資料
                context.MemberLogin.AddRange(
                     new MemberLogin
                     {
                         Account = "member001",
                         Password = "Aa123456",
                         MemberID = guidMember[0]
                     },
                     new MemberLogin
                     {
                         Account = "member002",
                         Password = "Bb123456",
                         MemberID = guidMember[1]
                     },
                     new MemberLogin
                     {
                         Account = "member003",
                         Password = "Cc123456",
                         MemberID = guidMember[2]
                     }
                    );
                context.SaveChanges();


                context.AccountStatus.AddRange();
                context.SaveChanges();

                context.AccountStatus.AddRange();
                context.SaveChanges();

                context.AccountStatus.AddRange();
                context.SaveChanges();

                context.AccountStatus.AddRange();
                context.SaveChanges();
            }
        }
    }
}
