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

        public static async Task SeedMemories(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (context.Memories.Any())
            {
                return; // DB has been seeded
            }

            var adminUser = await userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                Console.WriteLine("Admin user not found for seeding memories.");
                return;
            }

            var memories = new List<Memory>
            {
                new Memory { Title = "家庭聚餐", Description = "美好的晚餐時光", Date = new DateTime(2024, 1, 15), ImagePath = "https://picsum.photos/seed/familydinner/800/600", ApplicationUserId = adminUser.Id },
                new Memory { Title = "海邊度假", Description = "陽光、沙灘、海浪", Date = new DateTime(2023, 7, 20), ImagePath = "https://picsum.photos/seed/beachvacation/800/600", ApplicationUserId = adminUser.Id },
                new Memory { Title = "生日派對", Description = "快樂的生日慶祝", Date = new DateTime(2024, 3, 10), ImagePath = "https://picsum.photos/seed/birthdayparty/800/600", ApplicationUserId = adminUser.Id },
                new Memory { Title = "登山之旅", Description = "征服高山，美景盡收眼底", Date = new DateTime(2023, 10, 5), ImagePath = "https://picsum.photos/seed/hikingtrip/800/600", ApplicationUserId = adminUser.Id },
                new Memory { Title = "寵物時光", Description = "與毛小孩的溫馨日常", Date = new DateTime(2024, 2, 28), ImagePath = "https://picsum.photos/seed/pettime/800/600", ApplicationUserId = adminUser.Id },
                new Memory { Title = "聖誕節", Description = "溫馨的聖誕夜", Date = new DateTime(2023, 12, 25), ImagePath = "https://picsum.photos/seed/christmas/800/600", ApplicationUserId = adminUser.Id },
                new Memory { Title = "畢業典禮", Description = "值得紀念的一刻", Date = new DateTime(2024, 6, 20), ImagePath = "https://picsum.photos/seed/graduation/800/600", ApplicationUserId = adminUser.Id },
                new Memory { Title = "公園野餐", Description = "陽光下的悠閒午後", Date = new DateTime(2024, 4, 1), ImagePath = "https://picsum.photos/seed/picnic/800/600", ApplicationUserId = adminUser.Id },
                new Memory { Title = "新年快樂", Description = "迎接新的一年", Date = new DateTime(2024, 1, 1), ImagePath = "https://picsum.photos/seed/newyear/800/600", ApplicationUserId = adminUser.Id },
                new Memory { Title = "家庭旅遊", Description = "探索新地方", Date = new DateTime(2023, 9, 12), ImagePath = "https://picsum.photos/seed/familytravel/800/600", ApplicationUserId = adminUser.Id },
                new Memory { Title = "烹飪樂趣", Description = "一起動手做美食", Date = new DateTime(2024, 5, 3), ImagePath = "https://picsum.photos/seed/cooking/800/600", ApplicationUserId = adminUser.Id },
                new Memory { Title = "藝術創作", Description = "發揮創意，享受藝術", Date = new DateTime(2023, 11, 11), ImagePath = "https://picsum.photos/seed/art/800/600", ApplicationUserId = adminUser.Id }
            };

            await context.Memories.AddRangeAsync(memories);
            await context.SaveChangesAsync();
        }
    }
}
