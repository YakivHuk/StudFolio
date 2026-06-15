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
        // Для системних шляхів куки авторизації краще теж використовувати малі літери
        options.LoginPath = "/account/login";
        options.LogoutPath = "/account/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.AppendTrailingSlash = false;
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
        // Логування помилки за потреби
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/home/error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// --- ВЛАСНИЙ MIDDLEWARE ДЛЯ SEO-РЕДИРЕКТІВ (НИЖНІЙ РЕГІСТР ТА СЛЕШІ) ---
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;

    // Ігноруємо перевірку для статичних файлів, щоб не ламати MapStaticAssets
    if (!string.IsNullOrEmpty(path) && !path.Contains('.') && !path.StartsWith("/_"))
    {
        bool hasUppercase = path.Any(char.IsUpper);
        bool hasTrailingSlash = path.Length > 1 && path.EndsWith('/');

        if (hasUppercase || hasTrailingSlash)
        {
            var newPath = path.ToLowerInvariant();
            if (hasTrailingSlash)
            {
                newPath = newPath.TrimEnd('/');
            }

            // Формуємо новий URL з QueryString (якщо є параметри, наприклад ?id=5)
            var newUrl = newPath + context.Request.QueryString;

            context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
            context.Response.Headers.Location = newUrl;
            return; // Перериваємо конвеєр і відправляємо 301 редирект
        }
    }

    await next();
});

// Кастомний аналог обробки 404 помилки
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
    {
        context.Request.Path = "/home/error404";
        await next();
    }
});

app.MapStaticAssets();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();