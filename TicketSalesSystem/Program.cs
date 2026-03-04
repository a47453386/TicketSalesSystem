using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service;
using TicketSalesSystem.Service.ID;
using TicketSalesSystem.Service.Images;
using TicketSalesSystem.Service.IProgramme;
using TicketSalesSystem.Service.IUserAccessor;
using TicketSalesSystem.Service.Orders;
using TicketSalesSystem.Service.Queue;
using TicketSalesSystem.Service.Seats;
using TicketSalesSystem.Service.Sms;
using TicketSalesSystem.Service.SystemMonitor;
using TicketSalesSystem.Service.User;
using TicketSalesSystem.Service.Validation.IBookingValidation;
using TicketSalesSystem.Service.Validation.IProgrammeValidationService;
using TicketSalesSystem.Service.Validation.NewFolder;



var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://*:5098");

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



//註冊GuestBookContext類別
builder.Services.AddDbContext<TicketsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TicketsConnection")));

//註冊背景服務:清理過期位子
builder.Services.AddHostedService<Background>();

//註冊自動配位服務
builder.Services.AddScoped<ISeatService, SeatService>();

//註冊預定服務
builder.Services.AddScoped<IBookingValidationService, BookingValidationService>();


//註冊檔案上傳服務
builder.Services.AddScoped<IFileService, FileService>();

//註冊ID編碼服務
builder.Services.AddScoped<IIDService, IDService>();

//註冊活動編輯服務
builder.Services.AddScoped<IProgrammeService, ProgrammeEditService>();

//註冊訂單服務
builder.Services.AddScoped<IOrderService, OrderService>();

//註冊虛擬簡訊介面
builder.Services.AddScoped<ISmsService, MockSmsService>();

//註冊活動驗證服務
builder.Services.AddScoped<IProgrammeValidationService, ProgrammeValidationService>();

//註冊MemoryCache服務
builder.Services.AddMemoryCache();

//註冊佇列服務
builder.Services.AddScoped<IQueueService, QueueService>();


builder.Services.AddHttpContextAccessor();

//註冊帳號比對服務
builder.Services.AddScoped<IUserAccessorService, UserAccessorService>();

//註冊密碼複雜度服務
builder.Services.AddScoped<PasswordHasher<MemberLogin>>();

// 註冊為 Singleton，全站共用同一個記憶體空間
builder.Services.AddSingleton<SystemMonitorService>();

//註冊資料保護服務，並指定金鑰存放在專案資料夾下的一個特定目錄
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"./MyKeys/")) // 存放金鑰的資料夾
    .SetApplicationName("TicketSalesSystem");               // 設定應用程式辨識名稱

//註冊使用者服務
builder.Services.AddScoped<IUser,UserService>();


//註冊 Session 服務 加入 Session 服務

//提供內部記憶體快取(for流量)
builder.Services.AddDistributedMemoryCache();

// 設定 Cookie 認證
builder.Services.AddAuthentication()
    .AddCookie("MemberScheme", options =>
    {
        options.LoginPath = "/Login/MemberLogin"; // 會員登入路徑
        options.Cookie.Name = "TicketSystem.Member.Cookie"; // 這是會員的口袋
    })
    .AddCookie("EmployeeScheme", options =>
    {
        options.LoginPath = "/Login/EmployeeLogin"; // 員工登入路徑
        options.Cookie.Name = "TicketSystem.Employee.Cookie"; // 這是員工的口袋
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 設定 Session 過期時間（例如 30 分鐘）
    options.Cookie.HttpOnly = true;                // 增加安全性
    options.Cookie.IsEssential = true;             // 確保在沒同意 Cookie 政策下也能運作
});





//註冊BookingService
builder.Services.AddScoped<BookingService>();






var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    SeedData.Initialize(scope.ServiceProvider);
//}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 呼叫你的 SeedData 類別
        //SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "種子資料植入過程中發生錯誤！");
    }
}

// 2. 啟用介面 (在 app.Run() 之前)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // 預設路徑通常是 /swagger/index.html
}

// ... 其他中間件 (如 StaticFiles, Routing)
app.UseStaticFiles();

app.UseRouting();

// 啟用 Session (必須放在 UseRouting 之後，UseAuthorization 之前)
app.UseSession();

app.UseAuthentication();// 認證：你是誰？
app.UseAuthorization();// 授權：你能做什麼？

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
