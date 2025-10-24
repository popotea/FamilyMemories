using FamilyMemories.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyMemories.Data
{
    public static class DatabaseInitializer
    {
        public static async Task SeedUsersAndRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 確保角色存在
            var roles = new[] { "Admin", "User", "Guest" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new ApplicationRole
                    {
                        Name = roleName,
                        Description = $"{roleName} role in Family Memories app"
                    };
                    await roleManager.CreateAsync(role);
                }
            }

            // 創建預設管理員帳號 (admin/a123456)
            var adminEmail = "admin";
            var admin = await userManager.FindByNameAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = "admin@family.com",
                    FullName = "系統管理員",
                    AvatarPath = "/images/default-avatar.png", // 預設頭像路徑
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "a123456");
                if (result.Succeeded)
                {
                    // 確保管理員獲得 Admin 角色
                    await userManager.AddToRoleAsync(admin, "Admin");

                    // 創建更多預設角色權限演示
                    await userManager.AddToRoleAsync(admin, "User"); // 管理員同時擁有用戶權限
                }
                else
                {
                    // 記錄創建失敗的原因
                    Console.WriteLine("Failed to create admin user:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error: {error.Code} - {error.Description}");
                    }
                }
            }
            else
            {
                // 更新密碼，以防萬一
                var token = await userManager.GeneratePasswordResetTokenAsync(admin);
                var result = await userManager.ResetPasswordAsync(admin, token, "a123456");
                if (!result.Succeeded)
                {
                    Console.WriteLine("Failed to update admin password:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error: {error.Code} - {error.Description}");
                    }
                }
                else
                {
                    Console.WriteLine("Updated admin password");
                }
            }
        }
    }
}
