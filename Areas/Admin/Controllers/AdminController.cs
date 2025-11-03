using FamilyMemories.Data;
using FamilyMemories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FamilyMemories.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Dashboard 首頁
        public async Task<IActionResult> Index()
        {
            var stats = new
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                ActiveUsers = await _userManager.Users.CountAsync(u => u.IsActive),
                TotalMemories = await _context.Memories.CountAsync(),
                TotalRoles = await _roleManager.Roles.CountAsync()
            };

            ViewBag.Stats = stats;
            return View();
        }

        // 回憶管理
        public async Task<IActionResult> Memories()
        {
            var memories = await _context.Memories
                .Include(m => m.ApplicationUser)
                .OrderByDescending(m => m.Date)
                .ToListAsync();
            
            return View(memories);
        }

        // 刪除回憶
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMemory(int id)
        {
            var memory = await _context.Memories.FindAsync(id);
            if (memory == null)
            {
                return NotFound();
            }

            // 刪除實體檔案
            if (!string.IsNullOrEmpty(memory.ImagePath))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", memory.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Memories.Remove(memory);
            await _context.SaveChangesAsync();

            TempData["Success"] = "回憶已成功刪除";
            return RedirectToAction(nameof(Memories));
        }
    }
}
