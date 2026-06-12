using Microsoft.EntityFrameworkCore;
using StudFolio.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";   
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7); 
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await DbInitializer.InitializeAsync(context);
    }
    catch (Exception ex)
    {
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Кастомний аналог обробки 404 помилки
app.Use(async (context, next) =>
{
    await next();

    // Якщо конвеєр повернув 404 і відповідь користувачу ще не почала відправлятися
    if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
    {
        // Змінюємо шлях запиту на твій екшен помилки і виконуємо конвеєр знову
        context.Request.Path = "/Home/Error404";
        await next();
    }
});

app.MapStaticAssets();

app.UseStaticFiles();

app.UseRouting(); // Маршрутизація іде після статичних файлів та статус-кодів

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();