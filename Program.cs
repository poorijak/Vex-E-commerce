using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using System.Net.WebSockets;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<Customer>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddAuthentication().AddGoogle(googleOption =>
{
    googleOption.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    googleOption.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

    googleOption.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");
    googleOption.Scope.Add("https://www.googleapis.com/auth/userinfo.email");


    googleOption.ClaimActions.MapJsonKey("picture", "picture", "url");

});

var app = builder.Build();

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
            context.Response.Redirect("/Account/login");
            return;
        }

        using var scope = context.RequestServices.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Customer>>();

        var user = await userManager.GetUserAsync(context.User);

        if (user == null)
        {
            context.Response.Redirect("/Identity/Accout/login");
            return;
        }
        var role = user.Role;




        if (user.Role.ToString() != "Admin")
        {
            context.Response.Redirect("/");
            return;
        }
    }
    await next();

});



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

await SeedRole(app);



app.Run();


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


