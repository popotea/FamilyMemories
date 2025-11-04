using FamilyMemories.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FamilyMemories.Data
{
    public static class DatabaseInitializer
    {
        /// <summary>
        /// 修正 AspNetUsers.UserPermissions 與 AspNetRoles.RolePermissions 欄位內容，將 null 或空陣列設為 '{}'
        /// </summary>
        public static void FixPermissionsJsonFields(ApplicationDbContext context)
        {
            try
            {
                // 修正 AspNetUsers.UserPermissions 欄位
                // 使用 cardinality() 函數檢查 integer[] 陣列長度，而非 TRIM()
                context.Database.ExecuteSqlRaw(@"
UPDATE ""AspNetUsers""
SET ""UserPermissions"" = '{}'::integer[]
WHERE ""UserPermissions"" IS NULL OR cardinality(""UserPermissions"") = 0
");

                // 修正 AspNetRoles.RolePermissions 欄位
                context.Database.ExecuteSqlRaw(@"
UPDATE ""AspNetRoles""
SET ""RolePermissions"" = '{}'::integer[]
WHERE ""RolePermissions"" IS NULL OR cardinality(""RolePermissions"") = 0
");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"修正權限欄位時發生錯誤: {ex.Message}");
            }
        }

      public static async Task SeedUsersAndRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
   var logger = serviceProvider.GetService<ILogger<Program>>();

 try
     {
           // 確保資料庫已建立
          await context.Database.EnsureCreatedAsync();
          
         // 啟動時自動修正權限欄位內容
       FixPermissionsJsonFields(context);

  // 建立預設角色並分配權限
      await CreateDefaultRoles(roleManager, logger);

           // 建立預設管理員帳號
   await CreateDefaultAdmin(userManager, logger);

           // 建立預設一般用戶角色（供註冊用戶使用）
                await CreateDefaultUser(userManager, logger);

       logger?.LogInformation("資料庫初始化完成");
            }
            catch (Exception ex)
      {
      logger?.LogError(ex, "資料庫初始化時發生錯誤");
        Console.WriteLine($"資料庫初始化錯誤: {ex.Message}");
   }
        }

        private static async Task CreateDefaultRoles(RoleManager<ApplicationRole> roleManager, ILogger? logger)
   {
            // Admin 角色 - 擁有所有權限
  var adminPermissions = Enum.GetValues(typeof(Permission)).Cast<Permission>().ToList();
            await CreateRoleIfNotExists(roleManager, "Admin", "系統管理員 - 擁有所有權限", adminPermissions, logger);

            // User 角色 - 前台基本權限
      var userPermissions = new List<Permission>
          {
       Permission.前台_瀏覽首頁,
  Permission.前台_瀏覽回憶,
   Permission.前台_新增回憶,
  Permission.前台_編輯回憶,
                Permission.前台_刪除回憶,
      Permission.前台_上傳照片,
      Permission.前台_下載照片,
          Permission.前台_編輯個人資料,
                Permission.前台_檢視相簿
};
            await CreateRoleIfNotExists(roleManager, "User", "一般用戶 - 基本前台權限", userPermissions, logger);

     // Guest 角色 - 只能瀏覽
   var guestPermissions = new List<Permission>
            {
    Permission.前台_瀏覽首頁,
    Permission.前台_瀏覽回憶,
             Permission.前台_檢視相簿,
                Permission.前台_下載照片
        };
       await CreateRoleIfNotExists(roleManager, "Guest", "訪客 - 只能瀏覽內容", guestPermissions, logger);

            // Moderator 角色 - 內容管理員
   var moderatorPermissions = new List<Permission>
            {
  Permission.前台_瀏覽首頁,
Permission.前台_瀏覽回憶,
       Permission.前台_新增回憶,
     Permission.前台_編輯回憶,
                Permission.前台_刪除回憶,
     Permission.前台_上傳照片,
           Permission.前台_下載照片,
     Permission.前台_編輯個人資料,
       Permission.前台_檢視相簿,
        Permission.後台_管理回憶,
     Permission.後台_管理照片,
   Permission.後台_管理留言
     };
await CreateRoleIfNotExists(roleManager, "Moderator", "內容管理員 - 可管理回憶和照片", moderatorPermissions, logger);
      }

     private static async Task CreateRoleIfNotExists(RoleManager<ApplicationRole> roleManager, string roleName, string description, List<Permission> permissions, ILogger? logger)
        {
   if (!await roleManager.RoleExistsAsync(roleName))
    {
  var role = new ApplicationRole
 {
             Name = roleName,
         Description = description,
           RolePermissions = permissions
            };

     var result = await roleManager.CreateAsync(role);
              if (result.Succeeded)
{
             logger?.LogInformation($"成功建立角色: {roleName}");
  Console.WriteLine($"成功建立角色: {roleName}");
    }
      else
                {
               logger?.LogError($"建立角色 {roleName} 失敗: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    Console.WriteLine($"建立角色 {roleName} 失敗:");
    foreach (var error in result.Errors)
{
             Console.WriteLine($"  {error.Code}: {error.Description}");
         }
      }
    }
  else
            {
         // 更新現有角色的權限
        var existingRole = await roleManager.FindByNameAsync(roleName);
       if (existingRole != null)
      {
         existingRole.RolePermissions = permissions;
 existingRole.Description = description;
                  await roleManager.UpdateAsync(existingRole);
      logger?.LogInformation($"更新角色權限: {roleName}");
            }
        }
 }

        private static async Task CreateDefaultAdmin(UserManager<ApplicationUser> userManager, ILogger? logger)
  {
          var adminEmail = "admin";
            var admin = await userManager.FindByNameAsync(adminEmail);
   var allPermissions = Enum.GetValues(typeof(Permission)).Cast<Permission>().ToList();

    if (admin == null)
          {
                admin = new ApplicationUser
                {
    UserName = adminEmail,
        Email = "admin@familymemories.com",
            FullName = "系統管理員",
                 AvatarPath = "/images/default-avatar.png",
      EmailConfirmed = true,
          IsActive = true,
    UserPermissions = allPermissions,
      CreatedAt = DateTime.UtcNow
      };

       var result = await userManager.CreateAsync(admin, "Admin123!");
   if (result.Succeeded)
   {
  await userManager.AddToRoleAsync(admin, "Admin");
   await userManager.AddToRoleAsync(admin, "User");
       
      logger?.LogInformation("成功建立預設管理員帳號");
            Console.WriteLine("=== 預設管理員帳號已建立 ===");
         Console.WriteLine($"帳號: {admin.UserName}");
      Console.WriteLine($"密碼: Admin123!");
               Console.WriteLine($"Email: {admin.Email}");
    Console.WriteLine("========================");
}
         else
        {
  logger?.LogError($"建立管理員帳號失敗: {string.Join(", ", result.Errors.Select(e => e.Description))}");
 Console.WriteLine("建立管理員帳號失敗:");
   foreach (var error in result.Errors)
       {
         Console.WriteLine($"  {error.Code}: {error.Description}");
      }
         }
}
   else
            {
       // 確保現有管理員有完整權限
    admin.UserPermissions = allPermissions;
 admin.IsActive = true;
                await userManager.UpdateAsync(admin);
              
    // 確保在 Admin 角色中
      if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
  await userManager.AddToRoleAsync(admin, "Admin");
    }
   if (!await userManager.IsInRoleAsync(admin, "User"))
       {
   await userManager.AddToRoleAsync(admin, "User");
       }
     
            logger?.LogInformation("更新現有管理員權限");
          Console.WriteLine("已更新現有管理員權限");
        }
   }

   private static async Task CreateDefaultUser(UserManager<ApplicationUser> userManager, ILogger? logger)
        {
         var testUserEmail = "testuser";
  var testUser = await userManager.FindByNameAsync(testUserEmail);

   if (testUser == null)
            {
  var userPermissions = new List<Permission>
         {
      Permission.前台_瀏覽首頁,
                Permission.前台_瀏覽回憶,
     Permission.前台_新增回憶,
               Permission.前台_編輯回憶,
   Permission.前台_刪除回憶,
          Permission.前台_上傳照片,
      Permission.前台_下載照片,
      Permission.前台_編輯個人資料,
         Permission.前台_檢視相簿
   };

     testUser = new ApplicationUser
        {
         UserName = testUserEmail,
     Email = "testuser@familymemories.com",
          FullName = "測試用戶",
                 AvatarPath = "/images/default-avatar.png",
  EmailConfirmed = true,
    IsActive = true,
   UserPermissions = userPermissions,
   CreatedAt = DateTime.UtcNow
         };

      var result = await userManager.CreateAsync(testUser, "User123!");
if (result.Succeeded)
           {
 await userManager.AddToRoleAsync(testUser, "User");
       
           logger?.LogInformation("成功建立測試用戶帳號");
    Console.WriteLine("=== 測試用戶帳號已建立 ===");
     Console.WriteLine($"帳號: {testUser.UserName}");
     Console.WriteLine($"密碼: User123!");
             Console.WriteLine($"Email: {testUser.Email}");
   Console.WriteLine("========================");
                }
            else
            {
       logger?.LogError($"建立測試用戶帳號失敗: {string.Join(", ", result.Errors.Select(e => e.Description))}");
  Console.WriteLine("建立測試用戶帳號失敗:");
           foreach (var error in result.Errors)
     {
              Console.WriteLine($"  {error.Code}: {error.Description}");
      }
      }
  }
        }

        public static async Task SeedMemories(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
        // 不再自動新增假資料，讓使用者透過前後台介面自行新增回憶
      await Task.CompletedTask;
        }

        /// <summary>
        /// 強制重新初始化資料庫（開發用）
        /// </summary>
        public static async Task ForceReinitializeDatabase(IServiceProvider serviceProvider)
        {
   var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
          var logger = serviceProvider.GetService<ILogger<Program>>();

            try
            {
        // 刪除並重建資料庫
                await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();

          logger?.LogWarning("資料庫已重新初始化");
  Console.WriteLine("資料庫已重新初始化");

  // 重新建立所有資料
  await SeedUsersAndRoles(serviceProvider);
      }
    catch (Exception ex)
        {
   logger?.LogError(ex, "強制重新初始化資料庫時發生錯誤");
       Console.WriteLine($"強制重新初始化資料庫錯誤: {ex.Message}");
            }
        }
    }
}
