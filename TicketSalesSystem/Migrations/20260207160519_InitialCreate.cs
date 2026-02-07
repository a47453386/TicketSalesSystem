using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSalesSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountStatus",
                columns: table => new
                {
                    AccountStatusID = table.Column<string>(type: "nchar(1)", nullable: false),
                    AccountStatusName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountStatus", x => x.AccountStatusID);
                });

            migrationBuilder.CreateTable(
                name: "FAQPublishStatus",
                columns: table => new
                {
                    FAQPublishStatusID = table.Column<string>(type: "nchar(1)", nullable: false),
                    FAQPublishStatusName = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FAQPublishStatus", x => x.FAQPublishStatusID);
                });

            migrationBuilder.CreateTable(
                name: "FAQType",
                columns: table => new
                {
                    FAQTypeID = table.Column<string>(type: "nchar(2)", nullable: false),
                    FAQTypeName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FAQType", x => x.FAQTypeID);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatus",
                columns: table => new
                {
                    OrderStatusID = table.Column<string>(type: "nchar(1)", nullable: false),
                    OrderStatusName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatus", x => x.OrderStatusID);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethod",
                columns: table => new
                {
                    PaymentMethodID = table.Column<string>(type: "nchar(1)", nullable: false),
                    PaymentMethodName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethod", x => x.PaymentMethodID);
                });

            migrationBuilder.CreateTable(
                name: "Place",
                columns: table => new
                {
                    PlaceID = table.Column<string>(type: "nchar(1)", nullable: false),
                    PlaceName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PlaceAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VenueImage = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Place", x => x.PlaceID);
                });

            migrationBuilder.CreateTable(
                name: "ProgrammeStatus",
                columns: table => new
                {
                    ProgrammeStatusID = table.Column<string>(type: "nchar(1)", nullable: false),
                    ProgrammeStatusName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgrammeStatus", x => x.ProgrammeStatusID);
                });

            migrationBuilder.CreateTable(
                name: "QuestionType",
                columns: table => new
                {
                    QuestionTypeID = table.Column<string>(type: "nchar(2)", nullable: false),
                    QuestionTypeName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionType", x => x.QuestionTypeID);
                });

            migrationBuilder.CreateTable(
                name: "ReplyStatus",
                columns: table => new
                {
                    ReplyStatusID = table.Column<string>(type: "nchar(1)", nullable: false),
                    ReplyStatusName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplyStatus", x => x.ReplyStatusID);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleID = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RoleDescription = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "TicketsAreaStatus",
                columns: table => new
                {
                    TicketsAreaStatusID = table.Column<string>(type: "nchar(1)", nullable: false),
                    TicketsAreaStatusName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketsAreaStatus", x => x.TicketsAreaStatusID);
                });

            migrationBuilder.CreateTable(
                name: "TicketsStatus",
                columns: table => new
                {
                    TicketsStatusID = table.Column<string>(type: "nchar(1)", nullable: false),
                    TicketsStatusName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketsStatus", x => x.TicketsStatusID);
                });

            migrationBuilder.CreateTable(
                name: "VenueStatus",
                columns: table => new
                {
                    VenueStatusID = table.Column<string>(type: "nchar(1)", nullable: false),
                    VenueStatusName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueStatus", x => x.VenueStatusID);
                });

            migrationBuilder.CreateTable(
                name: "Member",
                columns: table => new
                {
                    MemberID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tel = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Gender = table.Column<bool>(type: "bit", nullable: false),
                    NationalID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPhoneVerified = table.Column<bool>(type: "bit", nullable: false),
                    AccountStatusID = table.Column<string>(type: "nchar(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Member", x => x.MemberID);
                    table.ForeignKey(
                        name: "FK_Member_AccountStatus_AccountStatusID",
                        column: x => x.AccountStatusID,
                        principalTable: "AccountStatus",
                        principalColumn: "AccountStatusID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    EmployeeID = table.Column<string>(type: "nchar(6)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tel = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Gender = table.Column<bool>(type: "bit", nullable: false),
                    NationalID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    Photo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RoleID = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    AccountStatusID = table.Column<string>(type: "nchar(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_Employee_AccountStatus_AccountStatusID",
                        column: x => x.AccountStatusID,
                        principalTable: "AccountStatus",
                        principalColumn: "AccountStatusID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employee_Role_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Role",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Venue",
                columns: table => new
                {
                    VenueID = table.Column<string>(type: "nchar(3)", nullable: false),
                    VenueName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FloorName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AreaColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RowCount = table.Column<int>(type: "int", nullable: false),
                    SeatCount = table.Column<int>(type: "int", nullable: false),
                    VenueStatusID = table.Column<string>(type: "nchar(1)", nullable: false),
                    PlaceID = table.Column<string>(type: "nchar(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venue", x => x.VenueID);
                    table.ForeignKey(
                        name: "FK_Venue_Place_PlaceID",
                        column: x => x.PlaceID,
                        principalTable: "Place",
                        principalColumn: "PlaceID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Venue_VenueStatus_VenueStatusID",
                        column: x => x.VenueStatusID,
                        principalTable: "VenueStatus",
                        principalColumn: "VenueStatusID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberLogin",
                columns: table => new
                {
                    MemberID = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Account = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberLogin", x => x.MemberID);
                    table.ForeignKey(
                        name: "FK_MemberLogin_Member_MemberID",
                        column: x => x.MemberID,
                        principalTable: "Member",
                        principalColumn: "MemberID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Question",
                columns: table => new
                {
                    QuestionID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    QuestionTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuestionDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadFile = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    MemberID = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    QuestionTypeID = table.Column<string>(type: "nchar(2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Question", x => x.QuestionID);
                    table.ForeignKey(
                        name: "FK_Question_Member_MemberID",
                        column: x => x.MemberID,
                        principalTable: "Member",
                        principalColumn: "MemberID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Question_QuestionType_QuestionTypeID",
                        column: x => x.QuestionTypeID,
                        principalTable: "QuestionType",
                        principalColumn: "QuestionTypeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeLogin",
                columns: table => new
                {
                    EmployeeID = table.Column<string>(type: "nchar(6)", nullable: false),
                    Account = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeLogin", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_EmployeeLogin_Employee_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employee",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FAQ",
                columns: table => new
                {
                    FAQID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    FAQTitle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FAQDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeID = table.Column<string>(type: "nchar(6)", nullable: false),
                    FAQPublishStatusID = table.Column<string>(type: "nchar(1)", nullable: false),
                    FAQTypeID = table.Column<string>(type: "nchar(2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FAQ", x => x.FAQID);
                    table.ForeignKey(
                        name: "FK_FAQ_Employee_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employee",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FAQ_FAQPublishStatus_FAQPublishStatusID",
                        column: x => x.FAQPublishStatusID,
                        principalTable: "FAQPublishStatus",
                        principalColumn: "FAQPublishStatusID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FAQ_FAQType_FAQTypeID",
                        column: x => x.FAQTypeID,
                        principalTable: "FAQType",
                        principalColumn: "FAQTypeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Programme",
                columns: table => new
                {
                    ProgrammeID = table.Column<string>(type: "nchar(8)", nullable: false),
                    ProgrammeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProgrammeDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CoverImage = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    SeatImage = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    LimitPerOrder = table.Column<int>(type: "int", nullable: false),
                    OnShelfTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmployeeID = table.Column<string>(type: "nchar(6)", nullable: true),
                    PlaceID = table.Column<string>(type: "nchar(1)", nullable: true),
                    ProgrammeStatusID = table.Column<string>(type: "nchar(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programme", x => x.ProgrammeID);
                    table.ForeignKey(
                        name: "FK_Programme_Employee_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employee",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_Programme_Place_PlaceID",
                        column: x => x.PlaceID,
                        principalTable: "Place",
                        principalColumn: "PlaceID");
                    table.ForeignKey(
                        name: "FK_Programme_ProgrammeStatus_ProgrammeStatusID",
                        column: x => x.ProgrammeStatusID,
                        principalTable: "ProgrammeStatus",
                        principalColumn: "ProgrammeStatusID");
                });

            migrationBuilder.CreateTable(
                name: "PublicNotice",
                columns: table => new
                {
                    PublicNoticeID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    PublicNoticeTitle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PublicNoticeDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublicNoticeStatus = table.Column<bool>(type: "bit", nullable: false),
                    EmployeeID = table.Column<string>(type: "nchar(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicNotice", x => x.PublicNoticeID);
                    table.ForeignKey(
                        name: "FK_PublicNotice_Employee_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employee",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reply",
                columns: table => new
                {
                    ReplyID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ReplyDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeID = table.Column<string>(type: "nchar(6)", nullable: false),
                    QuestionID = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    ReplyStatusID = table.Column<string>(type: "nchar(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reply", x => x.ReplyID);
                    table.ForeignKey(
                        name: "FK_Reply_Employee_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employee",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reply_Question_QuestionID",
                        column: x => x.QuestionID,
                        principalTable: "Question",
                        principalColumn: "QuestionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reply_ReplyStatus_ReplyStatusID",
                        column: x => x.ReplyStatusID,
                        principalTable: "ReplyStatus",
                        principalColumn: "ReplyStatusID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DescriptionImage",
                columns: table => new
                {
                    DescriptionImageID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ProgrammeID = table.Column<string>(type: "nchar(8)", nullable: false),
                    DescriptionImageName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescriptionImage", x => new { x.ProgrammeID, x.DescriptionImageID });
                    table.ForeignKey(
                        name: "FK_DescriptionImage_Programme_ProgrammeID",
                        column: x => x.ProgrammeID,
                        principalTable: "Programme",
                        principalColumn: "ProgrammeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Session",
                columns: table => new
                {
                    SessionID = table.Column<string>(type: "char(10)", nullable: false),
                    SaleStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SaleEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProgrammeID = table.Column<string>(type: "nchar(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Session", x => x.SessionID);
                    table.ForeignKey(
                        name: "FK_Session_Programme_ProgrammeID",
                        column: x => x.ProgrammeID,
                        principalTable: "Programme",
                        principalColumn: "ProgrammeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    OrderID = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    OrderCreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentTradeNO = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    PaymentDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentStatus = table.Column<bool>(type: "bit", nullable: false),
                    PaidTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MemberID = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    PaymentMethodID = table.Column<string>(type: "nchar(1)", nullable: false),
                    OrderStatusID = table.Column<string>(type: "nchar(1)", nullable: false),
                    SessionID = table.Column<string>(type: "char(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.OrderID);
                    table.ForeignKey(
                        name: "FK_Order_Member_MemberID",
                        column: x => x.MemberID,
                        principalTable: "Member",
                        principalColumn: "MemberID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_OrderStatus_OrderStatusID",
                        column: x => x.OrderStatusID,
                        principalTable: "OrderStatus",
                        principalColumn: "OrderStatusID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_PaymentMethod_PaymentMethodID",
                        column: x => x.PaymentMethodID,
                        principalTable: "PaymentMethod",
                        principalColumn: "PaymentMethodID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_Session_SessionID",
                        column: x => x.SessionID,
                        principalTable: "Session",
                        principalColumn: "SessionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketsArea",
                columns: table => new
                {
                    TicketsAreaID = table.Column<string>(type: "nchar(12)", nullable: false),
                    TicketsAreaName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RowCount = table.Column<int>(type: "int", nullable: false),
                    SeatCount = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "money", nullable: false),
                    TicketsAreaStatusID = table.Column<string>(type: "nchar(1)", nullable: false),
                    VenueID = table.Column<string>(type: "nchar(3)", nullable: false),
                    SessionID = table.Column<string>(type: "char(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketsArea", x => x.TicketsAreaID);
                    table.ForeignKey(
                        name: "FK_TicketsArea_Session_SessionID",
                        column: x => x.SessionID,
                        principalTable: "Session",
                        principalColumn: "SessionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketsArea_TicketsAreaStatus_TicketsAreaStatusID",
                        column: x => x.TicketsAreaStatusID,
                        principalTable: "TicketsAreaStatus",
                        principalColumn: "TicketsAreaStatusID");
                    table.ForeignKey(
                        name: "FK_TicketsArea_Venue_VenueID",
                        column: x => x.VenueID,
                        principalTable: "Venue",
                        principalColumn: "VenueID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    TicketsID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    RowIndex = table.Column<int>(type: "int", nullable: false),
                    SeatIndex = table.Column<int>(type: "int", nullable: false),
                    RefundTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScannedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TicketsStatusID = table.Column<string>(type: "nchar(1)", nullable: false),
                    OrderID = table.Column<string>(type: "nvarchar(14)", nullable: false),
                    SessionID = table.Column<string>(type: "char(10)", nullable: false),
                    TicketsAreaID = table.Column<string>(type: "nchar(12)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.TicketsID);
                    table.ForeignKey(
                        name: "FK_Tickets_Order_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Order",
                        principalColumn: "OrderID");
                    table.ForeignKey(
                        name: "FK_Tickets_Session_SessionID",
                        column: x => x.SessionID,
                        principalTable: "Session",
                        principalColumn: "SessionID");
                    table.ForeignKey(
                        name: "FK_Tickets_TicketsArea_TicketsAreaID",
                        column: x => x.TicketsAreaID,
                        principalTable: "TicketsArea",
                        principalColumn: "TicketsAreaID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tickets_TicketsStatus_TicketsStatusID",
                        column: x => x.TicketsStatusID,
                        principalTable: "TicketsStatus",
                        principalColumn: "TicketsStatusID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employee_AccountStatusID",
                table: "Employee",
                column: "AccountStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_RoleID",
                table: "Employee",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLogin_Account",
                table: "EmployeeLogin",
                column: "Account",
                unique: true,
                filter: "[Account] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FAQ_EmployeeID",
                table: "FAQ",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_FAQ_FAQPublishStatusID",
                table: "FAQ",
                column: "FAQPublishStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_FAQ_FAQTypeID",
                table: "FAQ",
                column: "FAQTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Member_AccountStatusID",
                table: "Member",
                column: "AccountStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_MemberLogin_Account",
                table: "MemberLogin",
                column: "Account",
                unique: true,
                filter: "[Account] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Order_MemberID",
                table: "Order",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_Order_OrderStatusID",
                table: "Order",
                column: "OrderStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Order_PaymentMethodID",
                table: "Order",
                column: "PaymentMethodID");

            migrationBuilder.CreateIndex(
                name: "IX_Order_SessionID",
                table: "Order",
                column: "SessionID");

            migrationBuilder.CreateIndex(
                name: "IX_Programme_EmployeeID",
                table: "Programme",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Programme_PlaceID",
                table: "Programme",
                column: "PlaceID");

            migrationBuilder.CreateIndex(
                name: "IX_Programme_ProgrammeStatusID",
                table: "Programme",
                column: "ProgrammeStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_PublicNotice_EmployeeID",
                table: "PublicNotice",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Question_MemberID",
                table: "Question",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_Question_QuestionTypeID",
                table: "Question",
                column: "QuestionTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Reply_EmployeeID",
                table: "Reply",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Reply_QuestionID",
                table: "Reply",
                column: "QuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_Reply_ReplyStatusID",
                table: "Reply",
                column: "ReplyStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Session_ProgrammeID",
                table: "Session",
                column: "ProgrammeID");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrderID",
                table: "Tickets",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SessionID",
                table: "Tickets",
                column: "SessionID");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketsAreaID",
                table: "Tickets",
                column: "TicketsAreaID");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketsStatusID",
                table: "Tickets",
                column: "TicketsStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsArea_SessionID",
                table: "TicketsArea",
                column: "SessionID");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsArea_TicketsAreaStatusID",
                table: "TicketsArea",
                column: "TicketsAreaStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsArea_VenueID",
                table: "TicketsArea",
                column: "VenueID");

            migrationBuilder.CreateIndex(
                name: "IX_Venue_PlaceID",
                table: "Venue",
                column: "PlaceID");

            migrationBuilder.CreateIndex(
                name: "IX_Venue_VenueStatusID",
                table: "Venue",
                column: "VenueStatusID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DescriptionImage");

            migrationBuilder.DropTable(
                name: "EmployeeLogin");

            migrationBuilder.DropTable(
                name: "FAQ");

            migrationBuilder.DropTable(
                name: "MemberLogin");

            migrationBuilder.DropTable(
                name: "PublicNotice");

            migrationBuilder.DropTable(
                name: "Reply");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "FAQPublishStatus");

            migrationBuilder.DropTable(
                name: "FAQType");

            migrationBuilder.DropTable(
                name: "Question");

            migrationBuilder.DropTable(
                name: "ReplyStatus");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "TicketsArea");

            migrationBuilder.DropTable(
                name: "TicketsStatus");

            migrationBuilder.DropTable(
                name: "QuestionType");

            migrationBuilder.DropTable(
                name: "Member");

            migrationBuilder.DropTable(
                name: "OrderStatus");

            migrationBuilder.DropTable(
                name: "PaymentMethod");

            migrationBuilder.DropTable(
                name: "Session");

            migrationBuilder.DropTable(
                name: "TicketsAreaStatus");

            migrationBuilder.DropTable(
                name: "Venue");

            migrationBuilder.DropTable(
                name: "Programme");

            migrationBuilder.DropTable(
                name: "VenueStatus");

            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "Place");

            migrationBuilder.DropTable(
                name: "ProgrammeStatus");

            migrationBuilder.DropTable(
                name: "AccountStatus");

            migrationBuilder.DropTable(
                name: "Role");
        }
    }
}
