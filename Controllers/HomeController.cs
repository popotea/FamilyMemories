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

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var memories = await _context.Memories.OrderByDescending(m => m.Date).ToListAsync();

            foreach (var memory in memories)
            {
                _logger.LogInformation("Memory ImagePath: {ImagePath}", memory.ImagePath);
            }

            var photos = memories.Select(m => new PhotoViewModel
            {
                Id = m.Id,
                ImageUrl = m.ImagePath,
                Member = "", // Placeholder, as Memory model does not have MemberName
                Date = m.Date.ToString("yyyy年MM月dd日"),
                Title = m.Title,
                Description = m.Description
            }).ToList();

            var viewModel = new IndexViewModel
            {
                WelcomeMessage = "歡迎使用家庭回憶相簿",
                AppDescription = "記錄您最珍貴的家庭時刻",
                Photos = photos
            };

            return View(viewModel);
        }
    }
}
