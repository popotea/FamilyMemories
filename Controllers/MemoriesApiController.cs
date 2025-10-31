using FamilyMemories.Data;
using FamilyMemories.Models;
using FamilyMemories.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FamilyMemories.Controllers
{
    [Route("api/memories")]
    [ApiController]
    [Authorize]
    public class MemoriesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public MemoriesApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PhotoViewModel>>> GetMemories(int page = 1, int pageSize = 10)
        {
            var memories = await _context.Memories
                .Include(m => m.ApplicationUser)
                .OrderByDescending(m => m.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var photos = memories.Select(m => new PhotoViewModel
            {
                Id = m.Id,
                ImageUrl = m.ImagePath,
                Member = m.ApplicationUser?.FullName ?? m.ApplicationUser?.UserName ?? "未知",
                Date = m.Date.ToString("yyyy年MM月dd日"),
                Title = m.Title,
                Description = m.Description,
                UserId = m.ApplicationUserId
            }).ToList();

            return photos;
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateMemory([FromForm] int id, [FromForm] string title, [FromForm] string description, [FromForm] DateTime date, [FromForm] string member, [FromForm] IFormFile imageFile)
        {
            var memoryToUpdate = await _context.Memories.FindAsync(id);

            if (memoryToUpdate == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (memoryToUpdate.ApplicationUserId != currentUser.Id)
            {
                return Forbid();
            }

            memoryToUpdate.Title = title;
            memoryToUpdate.Description = description;
            memoryToUpdate.Date = date;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.FullName == member || u.UserName == member);
            if (user != null)
            {
                memoryToUpdate.ApplicationUserId = user.Id;
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                memoryToUpdate.ImagePath = "/images/" + uniqueFileName;
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Memory updated successfully." });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Memories.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
