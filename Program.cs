using Microsoft.EntityFrameworkCore;

//using QuickBus.Data;
using QuickBus.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
// Change to use SQL Server instead of InMemory
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("BusBookingDb"));

builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Seed admin and sample data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Users.Any(u => u.Role == "Admin"))
    {
        db.Users.Add(new User { Username = "admin", Password = "admin", Role = "Admin" });
        db.Counters.AddRange(
            new Counter { Name = "Counter A" },
            new Counter { Name = "Counter B" }
        );
        db.SaveChanges();
    }
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
