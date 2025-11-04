using FamilyMemories.Data;
using FamilyMemories.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// -----------------------------------------------------------------------------
// 主要啟動與設定說明（中文註解）
//
// 1) 設定檔：請在 `appsettings.Development.json` 或生產環境的設定中提供
//    CloudflareR2:ServiceUrl 與 CloudflareR2:Bucket。
//    AccessKeyId 與 SecretAccessKey 建議放在環境變數或 secret manager，避免出現在版本控制中。
//
// 2) 如果有註冊 Cloudflare R2 設定（在程式稍後會檢查配置），Program.cs 會註冊 AWSSDK 的 IAmazonS3
//    並註冊 FamilyMemories.Services.IStorageService 的實作 (CloudflareR2StorageService)。
//
// 3) 本專案上傳流程（詳見 Controllers/Api/MemoriesController.cs）：
//    - 若註冊了 IStorageService，上傳會把檔案上傳至 Cloudflare R2，並把回傳的 URL 存入資料庫的 ImagePath
//    - 若未註冊 IStorageService，會回落到本地 `wwwroot/uploads/`，並把檔名存入 ImagePath（向下相容）
//
// 4) 在本機測試上傳時：
//    - 可在 `appsettings.Development.json` 放入 ServiceUrl 與 Bucket（但不要把金鑰放在檔案中）
//    - 將 Access/Secret 設為系統環境變數 (Windows PowerShell 範例)：
//        $env:CF_R2_AK = "your-access-key"
//        $env:CF_R2_SK = "your-secret-key"
//    - 或在 CI / 主機上使用 Secret 管理服務
//
// 5) 啟動： dotnet run（或使用 IDE 的啟動設定），測試上傳 API（例如使用 Postman 對 POST /api/memories 上傳 form-data）
// -----------------------------------------------------------------------------

// Add services to the container.
builder.Services.AddControllersWithViews(); // MVC support
builder.Services.AddRazorPages(); // Keep Razor Pages for now (will migrate later)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Cloudflare R2 / S3-compatible storage registration
// Install AWSSDK.S3 package to enable IAmazonS3 usage.
var cf = builder.Configuration.GetSection("CloudflareR2");
if (cf.Exists())
{
  var accessKey = cf["AccessKeyId"];
    var secretKey = cf["SecretAccessKey"];
    var serviceUrl = cf["ServiceUrl"];

    if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey) && !string.IsNullOrEmpty(serviceUrl))
    {
        // Register IAmazonS3 configured for R2
        builder.Services.AddSingleton<Amazon.Runtime.AWSCredentials>(new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey));
        builder.Services.AddSingleton<Amazon.S3.IAmazonS3>(sp =>
        {
            var creds = sp.GetRequiredService<Amazon.Runtime.AWSCredentials>();
            var config = new Amazon.S3.AmazonS3Config
    {
 ServiceURL = serviceUrl,
     ForcePathStyle = true,
      SignatureVersion = "4"
      };
    return new Amazon.S3.AmazonS3Client(creds, config);
    });

// Register our storage service
   builder.Services.AddSingleton<FamilyMemories.Services.IStorageService, FamilyMemories.Services.CloudflareR2StorageService>();
    }
}

var app = builder.Build();

// 建立服務範圍來設定資料庫
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("開始資料庫初始化...");
        
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        
   // 檢查資料庫連線
        var canConnect = await dbContext.Database.CanConnectAsync();
        if (!canConnect)
        {
            logger.LogWarning("無法連接到資料庫，跳過資料庫初始化");
     Console.WriteLine("=== 警告 ===");
            Console.WriteLine("無法連接到資料庫");
    Console.WriteLine("請檢查 appsettings.json 中的連線字串設定");
            Console.WriteLine("應用程式將以無資料庫模式啟動");
      Console.WriteLine("============");
        }
        else
        {
          // 確保資料庫存在並應用遷移
    await dbContext.Database.MigrateAsync();
            logger.LogInformation("資料庫遷移完成");

            // 種子預設角色和用戶
await DatabaseInitializer.SeedUsersAndRoles(services);
            logger.LogInformation("角色和用戶種子化完成");

            // 種子預設回憶
var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
 await DatabaseInitializer.SeedMemories(dbContext, userManager);
  logger.LogInformation("回憶種子化完成");

       Console.WriteLine("=== 資料庫初始化成功 ===");
  Console.WriteLine("可使用以下帳號登入：");
            Console.WriteLine("管理員: admin / Admin123!");
          Console.WriteLine("測試用戶: testuser / User123!");
            Console.WriteLine("==================");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "資料庫初始化過程發生錯誤");
        Console.WriteLine("=== 資料庫初始化失敗 ===");
        Console.WriteLine($"錯誤訊息: {ex.Message}");
        Console.WriteLine("請檢查以下項目：");
        Console.WriteLine("1. 資料庫服務是否正在運行");
      Console.WriteLine("2. 連線字串是否正確");
  Console.WriteLine("3. 網路連線是否正常");
  Console.WriteLine("應用程式將繼續運行，但無法使用資料庫功能");
        Console.WriteLine("==================");
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

app.UseAuthentication(); // 確保在 UseAuthorization 之前
app.UseAuthorization();

// 全域授權: 預設要求所有頁面都需要驗證
// 只有明確標註 [AllowAnonymous] 的頁面才免登入
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.TrimEnd('/');
 if (string.IsNullOrEmpty(path)) path = "/";
    var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;

    // 始終允許訪問的路徑（無論登入狀態）
    var alwaysAllowedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
      "/",  // 首頁相簿
  "/Home",          // MVC Home controller
     "/Home/Index",          // MVC Home/Index action
   "/Memories/Index",    // Razor Page 相簿頁面 (備用)
        "/Account/Login",       // 登入頁面
        "/Account/Register",    // 註冊頁面
        "/popo/login",      // 自定義登入路由
        "/Identity/Account/Login",
        "/Identity/Account/Register",
        "/Identity/Account/ForgotPassword",
        "/Identity/Account/ResetPassword"
    };

    // 如果是靜態資源、API、或 Admin 區域，交由授權處理
    if (path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase) || 
    path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase) || 
        path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
  {
     await next();
        return;
    }

    // 如果路由已經被 UseRouting 配對到 endpoint（例如我們用 MapGet 註冊的 /popo/login），直接放行
    var endpoint = context.GetEndpoint();
    if (endpoint != null)
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

    // 如果未登入且不是始終允許的路徑，重導向到登入頁面
    if (!isAuthenticated)
    {
     context.Response.Redirect("/Account/Login");
   return;
}

    await next();
});

// Area routing (Admin backend)
app.MapControllerRoute(
    name: "areas",
  pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

// Default MVC routing (Frontend)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 自定義登入路由：/popo/login 映射到新的 MVC 登入頁面，直接導到 /Account/Login
app.MapGet("/popo/login", () => Results.Redirect("/Account/Login"));

// 將舊的 Razor Pages Identity 路徑導向到新的 MVC Account 路由，避免 404
app.MapGet("/Identity/Account/Login", () => Results.Redirect("/Account/Login"));
app.MapGet("/Identity/Account/Register", () => Results.Redirect("/Account/Register"));
app.MapGet("/Identity/Account/Logout", () => Results.Redirect("/Account/Login"));

// Keep Razor Pages for Identity pages (login, register, etc.)
app.MapRazorPages();

// 開發模式下的資料庫重置端點（僅供開發使用）
if (app.Environment.IsDevelopment())
{
    app.MapGet("/dev/reset-database", async (IServiceProvider serviceProvider) =>
    {
        await DatabaseInitializer.ForceReinitializeDatabase(serviceProvider);
        return Results.Ok("資料庫已重置並重新初始化");
    });
}

app.Run();
