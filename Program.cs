using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using System.Net.WebSockets;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;
using Vex_E_commerce.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<Customer>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

string Mask(string value)
{
    if (string.IsNullOrEmpty(value)) return "NULL";
    if (value.Length <= 5) return "*****";
    return value.Substring(0, 5) + "*****";
}

Console.WriteLine($"Google ClientId (masked): {Mask(googleClientId)}");
Console.WriteLine($"Google ClientSecret (masked): {Mask(googleClientSecret)}");



if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.AddAuthentication().AddGoogle(googleOption =>
    {
        googleOption.ClientId = googleClientId;
        googleOption.ClientSecret = googleClientSecret;
        googleOption.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");
        googleOption.Scope.Add("https://www.googleapis.com/auth/userinfo.email");
        googleOption.ClaimActions.MapJsonKey("picture", "picture", "url");
    });
}

builder.Services.AddHttpClient<CatboxServices>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    client.Timeout = TimeSpan.FromMinutes(2);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
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
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    if (path != null && path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Response.Redirect("/Account/Login");
            return;
        }

        using var scope = context.RequestServices.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Customer>>();
        var user = await userManager.GetUserAsync(context.User);

        if (user == null)
        {
            context.Response.Redirect("/Identity/Account/Login");
            return;
        }

        if (user.Role != UserRole.Admin)
        {
            context.Response.Redirect("/");
            return;
        }
    }

    await next();
});

// 3. Database Migration (Run ก่อน App start เสมอ)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // ระวัง: บน Azure ถ้า Database ยังไม่พร้อมหรือ Firewall บล็อค บรรทัดนี้จะใช้เวลานานจน Timeout ได้
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// 4. Seed Role (Run ก่อน App start)
await SeedRole(app);

// 5. Seed Data (Run ก่อน App start) - ย้ายมาจากข้างล่าง
using (var scope = app.Services.CreateScope())
{
    Vex_E_commerce.Data.DbInitializer.Seed(app);
}

// 6. บรรทัดสุดท้ายต้องเป็น Run เสมอ
app.Run();


// --- Helper Methods ---
async Task SeedRole(IHost app)
{
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Customer"))
        {
            await roleManager.CreateAsync(new IdentityRole("Customer"));
        }

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
    }
}