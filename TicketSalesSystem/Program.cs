using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service;
using TicketSalesSystem.Service.Seats;
using TicketSalesSystem.Service.Validation.IBookingValidation;
using TicketSalesSystem.Service.Validation.NewFolder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//註冊GuestBookContext類別
builder.Services.AddDbContext<TicketsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TicketsConnection")));

//註冊背景服務:清理過期位子
builder.Services.AddHostedService<Background>();

//註冊ISeatService及SeatService
builder.Services.AddScoped<ISeatService, SeatService>();

//註冊驗證服務
builder.Services.AddScoped<IBookingValidationService, BookingValidationService>();











var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    SeedData.Initialize(scope.ServiceProvider);
}





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
