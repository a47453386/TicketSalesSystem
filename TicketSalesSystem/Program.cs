using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service;
using TicketSalesSystem.Service.ID;
using TicketSalesSystem.Service.Images;
using TicketSalesSystem.Service.IProgramme;
using TicketSalesSystem.Service.Orders;
using TicketSalesSystem.Service.Seats;
using TicketSalesSystem.Service.Sms;
using TicketSalesSystem.Service.Validation.IBookingValidation;
using TicketSalesSystem.Service.Validation.IProgrammeValidationService;
using TicketSalesSystem.Service.Validation.NewFolder;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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





//註冊 Session 服務 加入 Session 服務
builder.Services.AddDistributedMemoryCache(); // 提供內部記憶體快取
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 設定 Session 過期時間（例如 30 分鐘）
    options.Cookie.HttpOnly = true;                // 增加安全性
    options.Cookie.IsEssential = true;             // 確保在沒同意 Cookie 政策下也能運作
});



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    //SeedData.Initialize(scope.ServiceProvider);
}



// ... 其他中間件 (如 StaticFiles, Routing)
app.UseStaticFiles();

app.UseRouting();

// 啟用 Session (必須放在 UseRouting 之後，UseAuthorization 之前)
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

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
