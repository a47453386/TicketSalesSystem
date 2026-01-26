using Microsoft.EntityFrameworkCore;
using System;

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
                string[] Image = { "20260101", "20260201", "20260301" };
                context.Programme.AddRange(
                    new Programme()
                    {
                        ProgrammeID = "20260101",
                        ProgrammeName = "五月天 2026 世界巡迴演唱會",
                        ProgrammeDescription = "五月天 2026 世界巡迴演唱會－台北站，經典曲目全新編排。",
                        CreatedTime =DateTime.Now,
                        UpdatedAt = null,
                        CoverImage = "C" + Image[0]+".jpg",
                        SeatImage = "S"+ Image[0] + ".jpg",
                        LimitPerOrder = 4,
                        EmployeeID = "P23001",
                        PlaceID = "B",
                        ProgrammeStatusID = "B"
                    },
                    new Programme
                    {
                        ProgrammeID = "20260201",
                        ProgrammeName = "頑童MJ116 OGS 台中洲際演唱會",
                        ProgrammeDescription = "睽違六年，OG再起——金曲嘻哈天團頑童MJ116重磅回歸！",
                        CreatedTime = DateTime.Now,
                        UpdatedAt = null,
                        CoverImage = "C" + Image[1] + ".jpg",
                        SeatImage = "S" + Image[1] + ".jpg",
                        LimitPerOrder = 6,
                        EmployeeID = "P23001",
                        PlaceID = "A",
                        ProgrammeStatusID = "B"
                    },
                    new Programme
                    {
                        ProgrammeID = "20260301",
                        ProgrammeName = "丁噹Della 《夜遊 A Night Tour》",
                        ProgrammeDescription = "丁噹Della 《夜遊 A Night Tour》高雄巨蛋夜未眠巡迴演唱會",
                        CreatedTime = DateTime.Now,
                        UpdatedAt = null,
                        CoverImage = "C" + Image[2] + ".jpg",
                        SeatImage = "S" + Image[2] + ".jpg",
                        LimitPerOrder = 2,
                        EmployeeID = "P23001",
                        PlaceID = "A",
                        ProgrammeStatusID = "A"
                    }
                );
                context.SaveChanges();
               
                //封面圖片
                string CSeedPhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "SeedPhotos", "CoverImage");
                string CCoverImagePhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CoverImage");

                string[] Cfiles = Directory.GetFiles(CSeedPhotoPath);

                for (int i = 0; i < Cfiles.Length; i++)
                {
                    var CfileName = "C"+Image[i] + ".jpg";
                    var CDestFile = Path.Combine(Image[i] + ".jpg");
                    File.Copy(Cfiles[i], CDestFile);
                }

                //座位圖片
                string SSeedPhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "SeedPhotos", "SeatImage");
                string SCoverImagePhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "SeatImage");

                string[] Sfiles = Directory.GetFiles(SSeedPhotoPath);

                for (int i = 0; i < Sfiles.Length; i++)
                {
                    var SfileName = "C" + Image[i] + ".jpg";
                    var SDestFile = Path.Combine(Image[i] + ".jpg");
                    File.Copy(Sfiles[i], SDestFile);
                }

                //活動說明圖片
                string[] guidDescriptionImage = { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
                context.DescriptionImage.AddRange(
                    new DescriptionImage
                    {
                        DescriptionImageID= guidDescriptionImage[0],
                        DescriptionImageName= guidDescriptionImage[0] +".jpg",
                        ProgrammeID = "20260101"
                    },
                    new DescriptionImage
                    {
                        DescriptionImageID = Guid.NewGuid().ToString(),
                        DescriptionImageName = guidDescriptionImage[1] + ".jpg",
                        ProgrammeID = "20260201"
                    },
                    new DescriptionImage
                    {
                        DescriptionImageID = Guid.NewGuid().ToString(),
                        DescriptionImageName = guidDescriptionImage[2] + ".jpg",
                        ProgrammeID = "20260301"
                    }
                    );
                context.SaveChanges();
                string DSeedPhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "SeedPhotos", "DescriptionImage");
                string DCoverImagePhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "DescriptionImage");

                string[] Dfiles = Directory.GetFiles(DSeedPhotoPath);

                for (int i = 0; i < Sfiles.Length; i++)
                {
                    var DfileName = guidDescriptionImage[i] + ".jpg";
                    var DDestFile = Path.Combine(guidDescriptionImage[i] + ".jpg");
                    File.Copy(Dfiles[i], DDestFile);
                }
                



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
                         RoleID = "A",
                         AccountStatusID = "A"
                     },
                     new Employee
                     {
                         EmployeeID = "C24001",
                         Name = "蔡昇宴",
                         HireDate = new DateTime(2024, 1, 10),
                         Address = "新北市板橋區永和路4巷7號",
                         Birthday = new DateTime(1980, 5, 27),
                         Tel = "0912000003",
                         Gender = true,
                         NationalID = "C345678901",
                         Email = "Masa@mayday.com",
                         Extension = "#103",
                         Photo = "F24001.jpg",
                         CreatedTime = DateTime.Now,
                         LastLoginTime = null,
                         RoleID = "C",
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
                    new Role { RoleID = "C", RoleName = "客服人員" }
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

                //問題
                string[] guidQuestion = { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
                context.Question.AddRange(
                     new Question
                     {
                         QuestionID = guidQuestion[0],
                         QuestionTitle = "無法完成訂單付款",
                         QuestionDescription = "我在結帳時畫面一直轉圈，請問該如何處理？",
                         CreatedTime = DateTime.Now,
                         UploadFile = null,
                         MemberID = guidMember[0],
                         QuestionTypeID = "Q1"
                     },
                    new Question
                    {
                        QuestionID = guidQuestion[1],
                        QuestionTitle = "活動是否可以退票",
                        QuestionDescription = "請問演唱會門票是否可以在活動前退票？",
                        CreatedTime = DateTime.Now,
                        UploadFile = null,
                        MemberID = guidMember[1],
                        QuestionTypeID = "Q3"
                    },
                    new Question
                    {
                        QuestionID = guidQuestion[2],
                        QuestionTitle = "訂單資料顯示異常",
                        QuestionDescription = "我的訂單已付款，但狀態顯示尚未完成。",
                        CreatedTime = DateTime.Now,
                        UploadFile = guidQuestion[2] + ".pdf",
                        MemberID = guidMember[2],
                        QuestionTypeID = "Q1"
                    }
                    );
                context.SaveChanges();

                //問題種類
                context.QuestionType.AddRange(
                    new QuestionType
                    {
                        QuestionTypeID = "Q1",
                        QuestionTypeName = "訂單問題"
                    },
                    new QuestionType
                    {
                        QuestionTypeID = "Q2",
                        QuestionTypeName = "付款問題"
                    },
                    new QuestionType
                    {
                        QuestionTypeID = "Q3",
                        QuestionTypeName = "票券問題"
                    }

                    );
                context.SaveChanges();

                //回覆表單
                context.Reply.AddRange(
                    new Reply
                    {
                        ReplyID = Guid.NewGuid().ToString(),
                        ReplyDescription = "您好，請嘗試重新整理頁面，或更換瀏覽器後再次付款，若仍有問題請回覆告知。",
                        CreatedTime = DateTime.Now,
                        Note = "已電話通知會員",
                        EmployeeID = "C24001",
                        QuestionID = guidQuestion[0],
                        ReplyStatusID = "Y"
                    },
                    new Reply
                    {
                        ReplyID = Guid.NewGuid().ToString(),
                        ReplyDescription = "此活動門票可於活動前 7 天申請退票，退票流程已寄送至您的信箱。",
                        CreatedTime = DateTime.Now,
                        Note = null,
                        EmployeeID = "C24001",
                        QuestionID = guidQuestion[1],
                        ReplyStatusID = "Y"
                    },
                     new Reply
                     {
                         ReplyID = Guid.NewGuid().ToString(),
                         ReplyDescription = "我們正在協助查詢訂單狀態，確認後會再回覆您，感謝耐心等候。",
                         CreatedTime = DateTime.Now,
                         Note = "需確認金流回傳狀態",
                         EmployeeID = "C24001",
                         QuestionID = guidQuestion[2],
                         ReplyStatusID = "N"
                     }
                    );
                context.SaveChanges();

                //回覆狀態
                context.ReplyStatus.AddRange(
                    new ReplyStatus
                    {
                        ReplyStatusID = "N",
                        ReplyStatusName = "未回覆"
                    },
                    new ReplyStatus
                    {
                        ReplyStatusID = "Y",
                        ReplyStatusName = "已回覆"
                    }
                    );
                context.SaveChanges();

                //FAQ
                string[] guidFAQ = { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
                context.FAQ.AddRange(
                    new FAQ
                    {
                        FAQID = guidFAQ[0],
                        FAQTitle = "如何查詢我的訂單紀錄？",
                        FAQDescription = "請登入會員後，至【會員中心 → 我的訂單】即可查看所有歷史訂單與狀態。",
                        EmployeeID = "A23025",
                        FAQPublishStatusID = "Y",
                        FAQTypeID = "F1"
                    },
                    new FAQ
                    {
                        FAQID = guidFAQ[1],
                        FAQTitle = "付款失敗該怎麼辦？",
                        FAQDescription = "若付款失敗，請確認信用卡是否有效，或改用其他付款方式再次嘗試。",
                        EmployeeID = "A23025",
                        FAQPublishStatusID = "Y",
                        FAQTypeID = "F2"
                    },
                     new FAQ
                     {
                         FAQID = guidFAQ[2],
                         FAQTitle = "忘記密碼要怎麼重設？",
                         FAQDescription = "請於登入頁面點選【忘記密碼】，依照系統指示完成密碼重設流程。",
                         EmployeeID = "A23025",
                         FAQPublishStatusID = "Y",
                         FAQTypeID = "F3"
                     }
                    );
                context.SaveChanges();

                //FAQ種類
                context.FAQType.AddRange(
                    new FAQType
                    {
                        FAQTypeID = "F1",
                        FAQTypeName = "訂票流程"
                    },
                    new FAQType
                    {
                        FAQTypeID = "F2",
                        FAQTypeName = "付款問題"
                    },
                    new FAQType
                    {
                        FAQTypeID = "F3",
                        FAQTypeName = "帳號相關"
                    }
                    );
                context.SaveChanges();

                //FAQ發佈狀態
                context.FAQPublishStatus.AddRange(
                    new FAQPublishStatus
                    {
                        FAQPublishStatusID = "N",
                        FAQPublishStatusName = "未發佈"
                    },
                    new FAQPublishStatus
                    {
                        FAQPublishStatusID = "Y",
                        FAQPublishStatusName = "已發佈"
                    }
                    );
                context.SaveChanges();

                //公告
                string[] guidPublicNotice = { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
                context.PublicNotice.AddRange(
                     new PublicNotice
                     {
                         PublicNoticeID = guidPublicNotice[0],
                         PublicNoticeTitle = "系統維護公告",
                         PublicNoticeDescription = "本系統將於 2026 年 1 月 25 日 02:00 至 04:00 進行系統維護，期間暫停所有購票與查詢服務。",
                         CreatedTime = DateTime.Now,
                         UpdatedAt = null,
                         PublicNoticeStatus = true,
                         EmployeeID = "A23025"
                     },
                     new PublicNotice
                     {
                         PublicNoticeID = guidPublicNotice[1],
                         PublicNoticeTitle = "春節期間客服服務時間調整",
                         PublicNoticeDescription = "春節連假期間，客服服務時間調整為每日 10:00 至 16:00，造成不便敬請見諒。",
                         CreatedTime = DateTime.Now,
                         UpdatedAt = null,
                         PublicNoticeStatus = true,
                         EmployeeID = "A23025"
                     },
                     new PublicNotice
                     {
                         PublicNoticeID = guidPublicNotice[2],
                         PublicNoticeTitle = "新功能預告",
                         PublicNoticeDescription = "我們即將推出快速選位功能，讓購票流程更加順暢，敬請期待！",
                         CreatedTime = DateTime.Now,
                         UpdatedAt = null,
                         PublicNoticeStatus = false,
                         EmployeeID = "A23025"
                     }
                    );
                context.SaveChanges();

                //訂單
                context.Order.AddRange(
                     new Order
                     {
                         OrderID = "202601200001",
                         OrderCreatedTime = DateTime.Now,
                         PaidTime = new DateTime(2026, 1, 20, 10, 35, 00),
                         MemberID = guidMember[0],
                         PaymentMethodID = "A",
                         OrderStatusID = "Y",
                         SessionID = "2026010101",
                         PaymentTradeNO = "C"
                     },
                     new Order
                     {
                         OrderID = "202601200002",
                         OrderCreatedTime = DateTime.Now,
                         PaidTime = null,
                         MemberID = guidMember[1],
                         PaymentMethodID = "A",
                         OrderStatusID = "N",
                         SessionID = "2026020101",
                         PaymentTradeNO = "C"
                     },
                     new Order
                     {
                         OrderID = "202601200003",
                         OrderCreatedTime = DateTime.Now,
                         PaidTime = new DateTime(2026, 1, 21, 14, 20, 00),
                         MemberID = guidMember[2],
                         PaymentMethodID = "C",
                         OrderStatusID = "Y",
                         SessionID = "2026030101",
                         PaymentTradeNO = "A"
                     }
                    );
                context.SaveChanges();

                //訂單狀態
                context.OrderStatus.AddRange(
                    new OrderStatus
                    {
                        OrderStatusID = "N",
                        OrderStatusName = "未付款"
                    },
                    new OrderStatus
                    {
                        OrderStatusID = "Y",
                        OrderStatusName = "已付款"
                    }
);
                context.SaveChanges();

                //付款方式
                context.PaymentMethod.AddRange(
                    new PaymentMethod
                    {
                        PaymentMethodID = "C",
                        PaymentMethodName = "信用卡"
                    },
                    new PaymentMethod
                    {
                        PaymentMethodID = "A",
                        PaymentMethodName = "ATM 轉帳"
                    }
                    );
                context.SaveChanges();

                //金流
                string[] guidPayment = { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
                context.Payment.AddRange(
                    new Payment
                    {
                        PaymentTradeNO = guidPayment[0],
                        PaymentDescription = "信用卡付款成功",
                        PaymentStatusID = "Y",
                    },
                    new Payment
                    {
                        PaymentTradeNO = guidPayment[1],
                        PaymentDescription = "ATM 轉帳等待付款中",
                        PaymentStatusID = "W",
                    },
                    new Payment
                    {
                        PaymentTradeNO = guidPayment[2],
                        PaymentDescription = "ATM轉帳失敗",
                        PaymentStatusID = "N",
                    }

                    );
                context.SaveChanges();

                //金流狀態

                context.PaymentStatus.AddRange(
                    new PaymentStatus
                    {
                        PaymentStatusID = "W",
                        PaymentStatusName = "待付款"
                    },
                    new PaymentStatus
                    {
                        PaymentStatusID = "Y",
                        PaymentStatusName = "付款成功"
                    },
                    new PaymentStatus
                    {
                        PaymentStatusID = "N",
                        PaymentStatusName = "付款失敗"
                    }
                    );
                context.SaveChanges();

                //票券
                context.Tickets.AddRange(
                    new Tickets
                    {
                        TicketsID = "T000001",
                        CreatedTime = DateTime.Now,
                        ScannedTime = new DateTime(2026, 1, 10, 19, 30, 0),
                        RefundTime = null,
                        TicketsAreaID = "A01",
                        SeatID = "01",
                        RowID = "01",
                        TicketsStatusID = "N",
                        OrderID = "202601200001"
                    },
                    new Tickets
                    {
                        TicketsID = "T000002",

                        CreatedTime = new DateTime(2026, 1, 20, 11, 05, 00),
                        ScannedTime = new DateTime(2026, 1, 11, 19, 30, 0),
                        RefundTime = null,
                        TicketsAreaID = "B01",
                        SeatID ="01",
                        RowID = "02",
                        TicketsStatusID = "N",
                        OrderID = "202601200002"
                    },
                    new Tickets
                    {
                        TicketsID = "T000003",

                        CreatedTime = new DateTime(2026, 1, 21, 14, 30, 00),
                        ScannedTime = new DateTime(2026, 1, 12, 19, 30, 0),
                        RefundTime = null,
                        TicketsAreaID = "B01",
                        SeatID = "02",
                        RowID = "01",
                        TicketsStatusID = "N",
                        OrderID = "202601200003"
                    }
                    );
                context.SaveChanges();

                //票券狀態
                context.TicketsStatus.AddRange(
                     new TicketsStatus
                     {
                         TicketsStatusID = "N",
                         TicketsStatusName = "未使用"
                     },
                     new TicketsStatus
                     {
                         TicketsStatusID = "Y",
                         TicketsStatusName = "已使用"
                     }
                    );
                context.SaveChanges();

                //號
                context.Seat.AddRange(
                    new Seat
                    {
                        SeatID = "01",
                        SeatName = "01",
                        SeatStatusID = "A"
                    },
                    new Seat
                    {
                        SeatID = "02",
                        SeatName = "02",
                        SeatStatusID = "A"
                    },
                    new Seat
                    {
                        SeatID = "03",
                        SeatName = "03",
                        SeatStatusID = "B"
                    }
                    );
                context.SaveChanges();

                //座位狀態
                context.SeatStatus.AddRange(
                     new SeatStatus 
                     { 
                         SeatStatusID = "A",
                         SeatStatusName = "可選" 
                     },
                     new SeatStatus 
                     { 
                         SeatStatusID = "B", 
                         SeatStatusName = "已售出" 
                     },
                     new SeatStatus 
                     { 
                         SeatStatusID = "C",
                         SeatStatusName = "保留" 
                     }
                    );
                context.SaveChanges();

                //排
                context.SeatRow.AddRange(
                    new SeatRow
                    {
                        SeatRowID = "01",
                        SeatRowName = "01"                        
                    },
                    new SeatRow
                    {
                        SeatRowID = "02",
                        SeatRowName = "02"
                    },
                    new SeatRow
                    {
                        SeatRowID = "03",
                        SeatRowName = "03"
                    }

                    );
                context.SaveChanges();

                //票區
                context.TicketsArea.AddRange(
                    new TicketsArea
                    {
                        TicketsAreaID = "A01",
                        TicketsAreaName = "搖滾站區",
                        Price = 4800,
                        TicketsAreaStatusID ="O"
                    },
                    new TicketsArea
                    {
                        TicketsAreaID = "B01",
                        TicketsAreaName = "二樓看台區",
                        Price = 3200,
                        TicketsAreaStatusID ="I"
                    },
                    new TicketsArea
                    {
                        TicketsAreaID = "C01",
                        TicketsAreaName = "視線加強區",
                        Price = 1200,
                        TicketsAreaStatusID ="S"
                    }

                    );
                context.SaveChanges();

                //票區狀態
                context.TicketsAreaStatus.AddRange(
                    new TicketsAreaStatus 
                    { 
                        TicketsAreaStatusID = "I", 
                        TicketsAreaStatusName = "販售中" 
                    },
                    new TicketsAreaStatus
                    { 
                        TicketsAreaStatusID = "O",
                        TicketsAreaStatusName = "售完" 
                    },
                    new TicketsAreaStatus
                    {
                        TicketsAreaStatusID = "S",
                        TicketsAreaStatusName = "停止販售"
                    }
                     );
                context.SaveChanges();
            }
        }
    }
}
