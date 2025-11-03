using System.Diagnostics;
using FamilyMemories.Data;
using FamilyMemories.Models.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FamilyMemories.Models;

namespace FamilyMemories.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var memories = await _context.Memories
                .Include(m => m.ApplicationUser)
                .OrderByDescending(m => m.Date)
                .ToListAsync();

            var photos = memories.Select(m => new PhotoViewModel
            {
                Id = m.Id,
                ImageUrl = m.ImagePath,
                Member = m.ApplicationUser?.FullName ?? m.ApplicationUser?.UserName ?? "未知", // Display FullName, then UserName, then "未知"
                Date = m.Date.ToString("yyyy年MM月dd日"),
                Title = m.Title,
                Description = m.Description,
                UserId = m.ApplicationUserId
            }).ToList();

            // 取得當前使用者 ID，如果未登入則為 null
            var currentUserId = User?.Identity?.IsAuthenticated == true 
                ? _userManager.GetUserId(User) 
                : null;

            var viewModel = new IndexViewModel
            {
                WelcomeMessage = "歡迎使用家庭回憶相簿",
                AppDescription = "記錄您最珍貴的家庭時刻",
                Photos = photos,
                CurrentUserId = currentUserId
            };

            return View(viewModel);
        }
    }
}
