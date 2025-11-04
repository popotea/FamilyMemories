using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FamilyMemories.Data;
using FamilyMemories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using Microsoft.AspNetCore.Identity;

namespace FamilyMemories.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemoriesController : ControllerBase
    {
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly FamilyMemories.Services.IStorageService _storageService;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public MemoriesController(ApplicationDbContext context, IWebHostEnvironment env, UserManager<ApplicationUser> userManager, FamilyMemories.Services.IStorageService storageService)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
            _storageService = storageService;
        }

        // GET: api/memories - 允許匿名訪問，但只返回公開資訊
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MemoryDto>>> GetMemories()
        {
            var memories = await _context.Memories
                .Include(m => m.ApplicationUser)
                .OrderByDescending(m => m.Date)
                .Select(m => new MemoryDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    Date = m.Date,
                    ImagePath = m.ImagePath,
                    UserId = m.ApplicationUserId,
                    Member = m.ApplicationUser.FullName ?? m.ApplicationUser.UserName ?? "未知"
                })
                .ToListAsync();

            return Ok(memories);
        }

        // GET: api/memories/5 - 允許匿名訪問
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<MemoryDto>> GetMemory(int id)
        {
            var memory = await _context.Memories
                .Include(m => m.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (memory == null)
            {
                return NotFound();
            }

            var memoryDto = new MemoryDto
            {
                Id = memory.Id,
                Title = memory.Title,
                Description = memory.Description,
                Date = memory.Date,
                ImagePath = memory.ImagePath,
                UserId = memory.ApplicationUserId,
                Member = memory.ApplicationUser?.FullName ?? memory.ApplicationUser?.UserName ?? "未知"
            };

            return Ok(memoryDto);
        }

        // POST: api/memories - 必須登入
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<MemoryDto>> CreateMemory([FromForm] string title, [FromForm] string description, [FromForm] DateTime date, [FromForm] IFormFile file)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest(new { error = "標題不能為空" });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "請上傳圖片" });
            }

            // 驗證檔案
            if (file.Length > MaxFileSize)
                return BadRequest(new { error = $"檔案大小超過 {MaxFileSize / 1024 / 1024} MB 限制" });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                return BadRequest(new { error = "只允許圖片檔案 (.jpg, .jpeg, .png, .gif, .webp)" });

            if (_storageService == null)
            {
                return StatusCode(500, new { error = "圖片儲存服務未正確配置" });
            }

            // 上傳圖片到 Cloudflare R2
            string storedUrl;
            var fileName = $"{Guid.NewGuid()}{ext}";
            using (var stream = file.OpenReadStream())
            {
                var key = $"uploads/{fileName}";
                // UploadAsync 會回傳完整的公開 URL
                storedUrl = await _storageService.UploadAsync(stream, key, file.ContentType ?? "application/octet-stream");
            }

            // 取得當前使用者
            var userId = _userManager.GetUserId(User);

            // 確保日期是 UTC 格式，避免 PostgreSQL 時區錯誤
            var utcDate = date.Kind == DateTimeKind.Utc 
                ? date 
                : DateTime.SpecifyKind(date, DateTimeKind.Utc);

            var memory = new Memory
            {
                Title = title,
                Description = description,
                Date = utcDate, // 使用 UTC 日期
                ImagePath = storedUrl, // 儲存完整的 URL 而非 key
                ApplicationUserId = userId
            };

            _context.Memories.Add(memory);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(userId);

            var resultDto = new MemoryDto
            {
                Id = memory.Id,
                Title = memory.Title,
                Description = memory.Description,
                Date = memory.Date,
                ImagePath = memory.ImagePath,
                UserId = userId,
                Member = user?.FullName ?? user?.UserName ?? "未知"
            };

            return CreatedAtAction(nameof(GetMemory), new { id = memory.Id }, resultDto);
        }

        // PUT: api/memories/5 - 必須登入，且只能編輯自己的或管理員可以編輯所有
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<MemoryDto>> UpdateMemory(int id, [FromForm] string title, [FromForm] string description, [FromForm] IFormFile file)
        {
            var memory = await _context.Memories.Include(m => m.ApplicationUser).FirstOrDefaultAsync(m => m.Id == id);
            if (memory == null)
            {
                return NotFound();
            }

            // 權限檢查：只有記憶的擁有者或管理員可以編輯
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");
            if (memory.ApplicationUserId != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            // 更新標題和描述
            memory.Title = title;
            memory.Description = description;

            // 如果有上傳新圖片，處理圖片上傳
            if (file != null && file.Length > 0)
            {
                // 驗證檔案大小
                if (file.Length > MaxFileSize)
                    return BadRequest(new { error = $"檔案大小超過 {MaxFileSize / 1024 / 1024} MB 限制" });

                // 驗證副檔名
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                    return BadRequest(new { error = "只允許圖片檔案 (.jpg, .jpeg, .png, .gif, .webp)" });

                // 刪除舊圖片（如果存在）
                // 刪除舊圖片：若原本儲存在雲端（通常會是 URL 或包含 /），則透過 StorageService 刪除；
                // 否則把本地 uploads 資料夾的檔案刪除。
                if (!string.IsNullOrEmpty(memory.ImagePath))
                {
                    if (_storageService != null && (memory.ImagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase) || memory.ImagePath.Contains("/")))
                    {
                        // 若 ImagePath 是 URL 或 key，呼叫 StorageService 刪除（StorageService 會自行解析）
                        await _storageService.DeleteAsync(memory.ImagePath);
                    }
                }

                // 儲存新圖片
                var newFileName = $"{Guid.NewGuid()}{ext}";
                string newStoredUrl;
                using (var stream = file.OpenReadStream())
                {
                    var key = $"uploads/{newFileName}";
                    // UploadAsync 會回傳完整的公開 URL
                    newStoredUrl = await _storageService.UploadAsync(stream, key, file.ContentType ?? "application/octet-stream");
                }

                memory.ImagePath = newStoredUrl; // 儲存完整的 URL 而非 key
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // 回傳更新後的資料
            var resultDto = new MemoryDto
            {
                Id = memory.Id,
                Title = memory.Title,
                Description = memory.Description,
                Date = memory.Date,
                ImagePath = memory.ImagePath,
                UserId = memory.ApplicationUserId,
                Member = memory.ApplicationUser?.FullName ?? memory.ApplicationUser?.UserName ?? "未知"
            };

            return Ok(resultDto);
        }

        // DELETE: api/memories/5 - 必須登入，且只能刪除自己的或管理員可以刪除所有
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteMemory(int id)
        {
            var memory = await _context.Memories.FindAsync(id);
            if (memory == null)
            {
                return NotFound();
            }

            // 權限檢查：只有記憶的擁有者或管理員可以刪除
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");
            if (memory.ApplicationUserId != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            // 刪除記憶時同步刪除對應圖片：同樣支援雲端或本地檔案
            if (!string.IsNullOrEmpty(memory.ImagePath))
            {
                if (_storageService != null && memory.ImagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    // 呼叫 StorageService 刪除雲端物件
                    await _storageService.DeleteAsync(memory.ImagePath);
                }
            }

            _context.Memories.Remove(memory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MemoryExists(int id)
        {
            return _context.Memories.Any(e => e.Id == id);
        }

        // 移除不安全的 PATCH 和 批次更新端點
        // 這些端點允許未經適當授權的批次操作，存在安全風險
    }

    // DTO Classes
    public class MemoryDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string ImagePath { get; set; }
        public string UserId { get; set; }
        public string Member { get; set; }
    }
}
