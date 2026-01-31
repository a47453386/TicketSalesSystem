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
                //角色

                if (!context.Role.Any())
                {
                    context.Role.AddRange(
                    new Role
                    {
                        RoleID = "A",
                        RoleName = "系統管理員",
                        RoleDescription = ""
                    },
                    new Role
                    {
                        RoleID = "B",
                        RoleName = "活動管理員",
                        RoleDescription = ""
                    },
                    new Role
                    {
                        RoleID = "C",
                        RoleName = "客服人員",
                        RoleDescription = ""
                    }
                    );
                    context.SaveChanges();
                }

                //帳號狀態
                if (!context.AccountStatus.Any())
                {
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
                }

                //活動狀態
                if (!context.ProgrammeStatus.Any())
                {
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
                }
                //問題種類
                if (!context.QuestionType.Any())
                {
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
                }
                //回覆狀態
                if (!context.ReplyStatus.Any())
                {
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
                }
                //FAQ種類
                if (!context.FAQType.Any())
                {
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
                }
                //FAQ發佈狀態
                if (!context.FAQPublishStatus.Any())
                {
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
                }
                //訂單狀態
                if (!context.OrderStatus.Any())
                {
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
                }
                //付款方式
                if (!context.PaymentMethod.Any())
                {
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
                }
                //金流狀態
                if (!context.PaymentStatus.Any())
                {
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
                }
                //票券狀態
                if (!context.TicketsStatus.Any())
                {
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
                }
                //區域狀態
                if (!context.VenueStatus.Any())
                {
                    context.VenueStatus.AddRange(
                     new VenueStatus
                     {
                         VenueStatusID = "A",
                         VenueStatusName = "可使用"
                     },
                     new VenueStatus
                     {
                         VenueStatusID = "U",
                         VenueStatusName = "維修中"
                     },
                     new VenueStatus
                     {
                         VenueStatusID = "C",
                         VenueStatusName = "已關閉"
                     }
                    );
                    context.SaveChanges();
                }
                //票區狀態
                if (!context.TicketsAreaStatus.Any())
                {
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



                //場地
                string[] VImage = { "A", "B" };
                if (!context.Place.Any())
                {
                    context.Place.AddRange(
                    new Place
                    {
                        PlaceID = "A",
                        PlaceName = "台北小巨蛋",
                        PlaceAddress = "台北市松山區南京東路四段2號",
                        VenueImage= VImage[0]+ ".jpg"
                    },
                    new Place
                    {
                        PlaceID = "B",
                        PlaceName = "高雄巨蛋",
                        PlaceAddress = "高雄市左營區博愛二路757號",
                        VenueImage = VImage[1] + ".jpg"
                    }
                    );
                    context.SaveChanges();
                }
                //場內平面圖

                string VenueImageSeedPhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "SeedPhotos", "VenueImage");
                string VenueImagePhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "photos", "VenueImage");

                string[] VenueImageFiles = Directory.GetFiles(VenueImageSeedPhotoPath);

                for (int i = 0; i < VenueImageFiles.Length; i++)
                {
                    var VenueImageFileName = VImage[i] + ".jpg";
                    var VenueImageDestFile = Path.Combine(VenueImagePhotoPath, VenueImageFileName);
                    File.Copy(VenueImageFiles[i], VenueImageDestFile, overwrite: true);
                }


                //員工
                if (!context.Employee.Any())
                {
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
                         Photo = "P23001.jpg",
                         CreatedTime = DateTime.Now,
                         LastLoginTime = null,
                         RoleID = "B",
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
                         Photo = "A23025.jpg",
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
                         Photo = "C24001.jpg",
                         CreatedTime = DateTime.Now,
                         LastLoginTime = null,
                         RoleID = "C",
                         AccountStatusID = "A"
                     }

                    );
                    context.SaveChanges();
                }
                //員工帳號密碼
                if (!context.EmployeeLogin.Any())
                {
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
                        EmployeeID = "C24001"
                    }
                    );
                    context.SaveChanges();
                }
                //會員
                string[] guidMember = { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
                if (!context.Member.Any())
                {
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
                }
                //會員登入資料
                if (!context.MemberLogin.Any())
                {
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
                }

                //活動
                string[] Image = { "20260101", "20260201", "20260301" };
                if (!context.Programme.Any())
                {
                    context.Programme.AddRange(
                    new Programme()
                    {
                        ProgrammeID = "20260101",
                        ProgrammeName = "五月天 2026 世界巡迴演唱會",
                        ProgrammeDescription = "五月天 2026 世界巡迴演唱會－台北站，經典曲目全新編排。",
                        CreatedTime = DateTime.Now,
                        UpdatedAt = null,
                        CoverImage = "C" + Image[0] + ".jpg",
                        SeatImage = "S" + Image[0] + ".jpg",
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
                }

                //封面圖片

                string CoverImageSeedPhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "SeedPhotos", "CoverImage");
                string CoverImagePhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "photos", "CoverImage");

                string[] CoverImageFiles = Directory.GetFiles(CoverImageSeedPhotoPath);

                for (int i = 0; i < CoverImageFiles.Length; i++)
                {
                    var CoverImageFileName = "C" + Image[i] + ".jpg";
                    var CoverImageDestFile = Path.Combine(CoverImagePhotoPath, CoverImageFileName);
                    File.Copy(CoverImageFiles[i], CoverImageDestFile, overwrite: true);
                }

                //座位圖片
                string SeatImageSeedPhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "SeedPhotos", "SeatImage");
                string SeatImagePhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "photos", "SeatImage");

                string[] SeatImageFiles = Directory.GetFiles(SeatImageSeedPhotoPath);

                for (int i = 0; i < SeatImageFiles.Length; i++)
                {
                    var SeatImageFileName = "S" + Image[i] + ".jpg";
                    var SeatImageDestFile = Path.Combine(SeatImagePhotoPath, SeatImageFileName);
                    File.Copy(SeatImageFiles[i], SeatImageDestFile, overwrite: true);
                }

               
                //活動說明圖片

                string[] guidDescriptionImage = { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
                if (!context.DescriptionImage.Any())
                {
                    context.DescriptionImage.AddRange(
                    new DescriptionImage
                    {
                        DescriptionImageID = guidDescriptionImage[0],
                        DescriptionImageName = guidDescriptionImage[0] + ".jpg",
                        ProgrammeID = "20260101"
                    },
                    new DescriptionImage
                    {
                        DescriptionImageID = guidDescriptionImage[1],
                        DescriptionImageName = guidDescriptionImage[1] + ".jpg",
                        ProgrammeID = "20260201"
                    },
                    new DescriptionImage
                    {
                        DescriptionImageID = guidDescriptionImage[2],
                        DescriptionImageName = guidDescriptionImage[2] + ".jpg",
                        ProgrammeID = "20260301"
                    }
                    );
                    context.SaveChanges();
                    string DescriptionImageSeedPhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "SeedPhotos", "DescriptionImage");
                    string DescriptionImagePhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photos", "DescriptionImage");

                    string[] files = Directory.GetFiles(DescriptionImageSeedPhotoPath);

                    for (int i = 0; i < files.Length; i++)
                    {
                        var fileName = guidDescriptionImage[i] + ".jpg";
                        var DestFile = Path.Combine(DescriptionImagePhotoPath, fileName);
                        File.Copy(files[i], DestFile, overwrite: true);
                    }
                }

                string[] guidQuestion = { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
                //問題
                if (!context.Question.Any())
                {
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
                }


                //回覆表單
                if (!context.Reply.Any())
                {
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
                }
                //FAQ
                string[] guidFAQ = { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
                if (!context.FAQ.Any())
                {
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
                }



                //公告
                string[] guidPublicNotice = { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
                if (!context.PublicNotice.Any())
                {
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
                    try
                    {
                        context.SaveChanges();
                    }
                    catch (DbUpdateException ex)
                    {
                        // 重點在 InnerException，這會顯示 SQL Server 的原生錯誤
                        var message = ex.InnerException?.Message ?? ex.Message;
                        Console.WriteLine("資料庫報錯內容: " + message);
                    }

                }

                //金流
                string[] guidPayment = { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
                if (!context.Payment.Any())
                {
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
                }
               
                
                //區域
                if (!context.Venue.Any())
                {
                    context.Venue.AddRange(
                    new Venue
                    {
                        VenueID = "A01",
                        VenueName = "搖滾 A 區",
                        FloorName= "一樓",
                        AreaColor= "藍區",
                        RowCount = 20,
                        SeatCount = 15,
                        VenueStatusID = "A",
                        PlaceID = "A"
                    },
                    new Venue
                    {
                        VenueID = "A02",
                        VenueName = "看台 B 區",
                        FloorName = "二樓",
                        AreaColor = "藍區",
                        RowCount = 30,
                        SeatCount = 20,
                        VenueStatusID = "A",
                        PlaceID = "A"
                    },
                    new Venue
                    {
                        VenueID = "A03",
                        VenueName = "維修封閉區",
                        FloorName = "五樓",
                        AreaColor = "藍區",
                        RowCount = 10,
                        SeatCount = 10,
                        VenueStatusID = "U",
                        PlaceID = "A"
                    }

                    );

                    try
                    {
                        context.SaveChanges();
                    }
                    catch (DbUpdateException ex)
                    {
                        // 重點在 InnerException，這會顯示 SQL Server 的原生錯誤
                        var message = ex.InnerException?.Message ?? ex.Message;
                        Console.WriteLine("資料庫報錯內容: " + message);
                    }
                }
                //場次
                if (!context.Session.Any())
                {
                    context.Session.AddRange(
                    new Session
                    {
                        SessionID = "2026010101",
                        SaleStartTime = new DateTime(2026, 12, 1, 12, 0, 0),
                        SaleEndTime = new DateTime(2027, 1, 9, 23, 59, 59),
                        StartTime = new DateTime(2027, 1, 10, 19, 30, 0),
                        ProgrammeID = "20260101",
                        VenueID = "A01"
                    },
                    new Session
                    {
                        SessionID = "2026020101",
                        SaleStartTime = new DateTime(2026, 12, 2, 12, 0, 0),
                        SaleEndTime = new DateTime(2027, 1, 10, 23, 59, 59),
                        StartTime = new DateTime(2027, 1, 11, 19, 30, 0),
                        ProgrammeID = "20260201",
                        VenueID = "A01"
                    },
                    new Session
                    {
                        SessionID = "2026030101",
                        SaleStartTime = new DateTime(2026, 12, 3, 12, 0, 0),
                        SaleEndTime = new DateTime(2027, 1, 11, 23, 59, 59),
                        StartTime = new DateTime(2027, 1, 12, 19, 30, 0),
                        ProgrammeID = "20260301",
                        VenueID = "A01"

                    }
                    );
                    try
                    {
                        context.SaveChanges();
                    }
                    catch (DbUpdateException ex)
                    {
                        // 重點在 InnerException，這會顯示 SQL Server 的原生錯誤
                        var message = ex.InnerException?.Message ?? ex.Message;
                        Console.WriteLine("資料庫報錯內容: " + message);
                    }
                }

                //票區
                if (!context.TicketsArea.Any())
                {
                    context.TicketsArea.AddRange(
                    new TicketsArea
                    {
                        TicketsAreaID = "A01",
                        TicketsAreaName = "搖滾站區",
                        Price = 4800,
                        TicketsAreaStatusID = "I",
                        ProgrammeID = "20260101",
                        VenueID = "A01"
                    },
                    new TicketsArea
                    {
                        TicketsAreaID = "B01",
                        TicketsAreaName = "二樓看台區",
                        Price = 3200,
                        TicketsAreaStatusID = "I",
                        ProgrammeID = "20260101",
                        VenueID = "A02"
                    },
                    new TicketsArea
                    {
                        TicketsAreaID = "C01",
                        TicketsAreaName = "視線加強區",
                        Price = 1200,
                        TicketsAreaStatusID = "I",
                        ProgrammeID = "20260101",
                        VenueID = "A02"
                    }

                    );

                    context.SaveChanges();
                }
                //訂單
                if (!context.Order.Any())
                {
                    context.Order.AddRange(
                     new Order
                     {
                         OrderID = "202612000001",
                         OrderCreatedTime = DateTime.Now,
                         PaidTime = new DateTime(2026, 12, 1, 16, 00, 00),
                         MemberID = guidMember[0],
                         PaymentMethodID = "A",
                         OrderStatusID = "Y",
                         SessionID = "2026010101",
                         PaymentTradeNO = guidPayment[0]
                     },
                     new Order
                     {
                         OrderID = "202612000002",
                         OrderCreatedTime = DateTime.Now,
                         PaidTime = null,
                         MemberID = guidMember[1],
                         PaymentMethodID = "A",
                         OrderStatusID = "N",
                         SessionID = "2026020101",
                         PaymentTradeNO = guidPayment[1]
                     },
                     new Order
                     {
                         OrderID = "202612000003",
                         OrderCreatedTime = DateTime.Now,
                         PaidTime = new DateTime(2026, 12, 3, 14, 20, 00),
                         MemberID = guidMember[2],
                         PaymentMethodID = "C",
                         OrderStatusID = "Y",
                         SessionID = "2026030101",
                         PaymentTradeNO = guidPayment[2]
                     }
                    );
                    context.SaveChanges();
                }

                //票券
                if (!context.Tickets.Any())
                {
                    context.Tickets.AddRange(
                    new Tickets
                    {
                        TicketsID = "T000001",
                        CreatedTime = DateTime.Now,
                        ScannedTime = new DateTime(2027, 1, 10, 16, 30, 0),
                        RefundTime = null,    
                        TicketsStatusID = "N",
                        OrderID = "202612000001"
                    },
                    new Tickets
                    {
                        TicketsID = "T000002",
                        CreatedTime = DateTime.Now,
                        ScannedTime = new DateTime(2027, 1, 11, 16, 30, 0),
                        RefundTime = null,
                        TicketsStatusID = "N",
                        OrderID = "202612000002"
                    },
                    new Tickets
                    {
                        TicketsID = "T000003",
                        CreatedTime = DateTime.Now,
                        ScannedTime = new DateTime(2027, 1, 12, 17, 30, 0),
                        RefundTime = null,                        
                        TicketsStatusID = "N",
                        OrderID = "202612000003"
                    }
                    );
                    context.SaveChanges();
                }





            }
        }
    }
}
