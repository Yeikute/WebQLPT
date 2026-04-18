using WebQLPT.Data;
using WebQLPT.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews(options =>
{
    // Require authenticated users by default for all controllers/actions
    var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// Configure DbContext with SQL Server (recommended for production)
// If SQL Server is not available, ensure it's properly configured or use SQLite as fallback
var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection");
var sqlitePathConfig = builder.Configuration["Sqlite:File"] ?? "Data/webqlpt.db";

if (!string.IsNullOrEmpty(defaultConn) && defaultConn.Contains("SQLEXPRESS"))
{
    // Use SQL Server if connection string is configured
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(
            defaultConn,
            sqlOptions => sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null));
    });

    Console.WriteLine("✅ Database: SQL Server (SQLEXPRESS)");
}
else
{
    // Fallback to SQLite only if no SQL Server connection string
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite($"Data Source={sqlitePathConfig}"));

    Console.WriteLine($"⚠️  Database: SQLite ({sqlitePathConfig})");
    Console.WriteLine("ℹ️  Tip: Configure SQL Server connection in appsettings.json for production use.");
}

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Auth}/{id?}")
    .WithStaticAssets();
// Seed roles and default admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = new[] { "Admin", "ChuTro", "KhachThue" };
    foreach (var role in roles)
    {
        var exists = await roleManager.RoleExistsAsync(role);
        if (!exists)
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // create default admin
    var adminEmail = builder.Configuration["AdminUser:Email"] ?? "admin@example.com";
    var adminPassword = builder.Configuration["AdminUser:Password"] ?? "Admin@123";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var adminFullName = builder.Configuration["AdminUser:FullName"] ?? "Administrator";
        adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = adminFullName };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
    else
    {
        // ensure admin has Admin role and basic protections (email confirmed, lockout disabled)
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        var needsUpdate = false;
        if (!adminUser.EmailConfirmed)
        {
            adminUser.EmailConfirmed = true;
            needsUpdate = true;
        }
        if (adminUser.LockoutEnabled)
        {
            adminUser.LockoutEnabled = false;
            needsUpdate = true;
        }
        if (needsUpdate)
        {
            await userManager.UpdateAsync(adminUser);
        }
    }
}

app.Run();
