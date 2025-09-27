using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Practica2.Data;
using Microsoft.Extensions.Caching.StackExchangeRedis; 
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.AspNetCore.Hosting; 
using Microsoft.Extensions.Logging;
using Practica2.Extensions; 

var builder = WebApplication.CreateBuilder(args);




var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    
    options.UseNpgsql(connectionString);
});

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


using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        
        await Practica2.Data.IdentitySeeder.SeedRolesAsync(serviceProvider);
        
        
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the identity roles or migrating the database.");
    }
}




if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseSession(); 
app.UseAuthorization();


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();