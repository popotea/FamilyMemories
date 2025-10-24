using FamilyMemories.Data;
using FamilyMemories.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // MVC support
builder.Services.AddRazorPages(); // Keep Razor Pages for now (will migrate later)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 添加 Identity 服務
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
    // 密碼設定
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // 使用者設定
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var app = builder.Build();

// 建立服務範圍來設定資料庫
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 應用資料庫遷移並種子化
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        // 種子預設角色和用戶
        await DatabaseInitializer.SeedUsersAndRoles(services);

        // 種子預設回憶
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        await DatabaseInitializer.SeedMemories(dbContext, userManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "資料庫種子化過程發生問題，但應用程式會繼續運行。您可以在應用程式運行後手動執行 dotnet ef database update 來建立資料庫。");
        // 繼續運行應用程式，不因為種子化錯誤而停止
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // app.UseExceptionHandler("/Home/Error"); // No longer needed
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 全域授權: 預設要求所有頁面都需要驗證
// 只有明確標註 [AllowAnonymous] 的頁面才免登入
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;

    // 始終允許訪問的路徑（無論登入狀態）
    var alwaysAllowedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
        "/",                    // 首頁相簿
        "/Home",                // MVC Home controller
        "/Home/Index",          // MVC Home/Index action
        "/Memories/Index",      // Razor Page 相簿頁面 (備用)
        "/popo/login",          // 自定義登入路由
        "/Identity/Account/Login",
        "/Identity/Account/Register",
        "/Identity/Account/ForgotPassword",
        "/Identity/Account/ResetPassword"
    };

    // 如果是靜態資源、API GET請求，始終允許
    if (path.StartsWith("/lib/") || path.StartsWith("/css/") || path.StartsWith("/js/") ||
        context.Request.Method == "GET" && path.StartsWith("/api/memories"))
    {
        await next();
        return;
    }

    // 如果是始終允許的路徑，不檢查登入狀態
    if (alwaysAllowedPaths.Contains(path))
    {
        await next();
        return;
    }

    // 如果未登入且不是始終允許的路徑，不暴露登入網址，重導向到首頁
    if (!isAuthenticated)
    {
        context.Response.Redirect("/");
        return;
    }

    await next();
});

// MVC routing first
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // Home page shows gallery

// 自定義登入路由：/popo/login 映射到標準登入頁面
app.MapGet("/popo/login", () => Results.Redirect("/Identity/Account/Login"));

// Keep Razor Pages for Identity pages (login, register, etc.)
app.MapRazorPages();

// Catch-all route to redirect any other URL to the home page
app.MapControllerRoute(
    name: "catch-all",
    pattern: "{*url}",
    defaults: new { controller = "Home", action = "Index" });

app.Run();
