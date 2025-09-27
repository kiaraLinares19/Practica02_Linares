// Program.cs

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Practica2.Data;
using Microsoft.Extensions.Caching.StackExchangeRedis; 


var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();


var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection") 
    ?? Environment.GetEnvironmentVariable("Redis_ConnectionString");

if (!string.IsNullOrEmpty(redisConnectionString))
{
    
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "Practica2_"; 
    });
    
    
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true; 
        options.Cookie.IsEssential = true;
    });
}
else
{
    
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession();
}



var app = builder.Build();


app.UseRouting();

app.UseSession(); 


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();