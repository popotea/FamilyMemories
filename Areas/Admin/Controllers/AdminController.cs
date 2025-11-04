using FamilyMemories.Data;
using FamilyMemories.Models;
using FamilyMemories.Models.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using FamilyMemories.Services;
using System.IO;
using Microsoft.Extensions.Logging;

namespace FamilyMemories.Areas.Admin.Controllers
{
        [Area("Admin")]
        [Authorize(Roles = "Admin")]
        public class AdminController : Controller
        {
            private readonly ApplicationDbContext _context;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<ApplicationRole> _roleManager;
            private readonly IStorageService _storageService;
            private readonly ILogger<AdminController> _logger;

            public AdminController(
                ApplicationDbContext context,
                UserManager<ApplicationUser> userManager,
                RoleManager<ApplicationRole> roleManager,
                IStorageService storageService,
                ILogger<AdminController> logger)
            {
                _context = context;
                _userManager = userManager;
                _roleManager = roleManager;
                _storageService = storageService;
                _logger = logger;
            }

            // 批次刪除回憶
            [HttpPost]
            public async Task<IActionResult> BatchDeleteMemories([FromForm] int[] ids)
            {
                if (ids == null || ids.Length == 0)
                    return BadRequest();

                var memories = await _context.Memories.Where(m => ids.Contains(m.Id)).ToListAsync();
                foreach (var memory in memories)
                {
                    // 刪除圖片檔案（支援雲端或本地）
                    if (!string.IsNullOrEmpty(memory.ImagePath))
                    {
                        if (_storageService != null && (memory.ImagePath.StartsWith("http", System.StringComparison.OrdinalIgnoreCase) || memory.ImagePath.Contains("/")))
                        {
                            await _storageService.DeleteAsync(memory.ImagePath);
                        }
                        else
                        {
                            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", memory.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(imagePath))
                            {
                                System.IO.File.Delete(imagePath);
                            }
                        }
                    }
                }
                _context.Memories.RemoveRange(memories);
                await _context.SaveChangesAsync();
                return Ok();
            }

            // 新增回憶 (GET)
            public IActionResult CreateMemory()
            {
                return View();
            }

            // 新增回憶 (POST)
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> CreateMemory(EditMemoryViewModel model)
            {
                try
                {
                    _logger.LogInformation("開始新增回憶");
                    
                    if (!ModelState.IsValid)
                    {
                        _logger.LogWarning("ModelState 驗證失敗");
                        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                        {
                            _logger.LogWarning($"驗證錯誤: {error.ErrorMessage}");
                        }
                        return View(model);
                    }

                    string imageUrl = null;
                    if (Request.Form.Files.Count > 0)
                    {
                        var file = Request.Form.Files["Image"];
                        _logger.LogInformation($"接收到檔案: {file?.FileName}, 大小: {file?.Length} bytes");
                        
                        if (file != null && file.Length > 0)
                        {
                            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                            var fileName = $"uploads/{Guid.NewGuid()}{ext}";
                            _logger.LogInformation($"準備上傳圖片: {fileName}");
                            
                            using (var stream = file.OpenReadStream())
                            {
                                // UploadAsync 會回傳完整的公開 URL
                                imageUrl = await _storageService.UploadAsync(stream, fileName, file.ContentType ?? "application/octet-stream");
                                _logger.LogInformation($"圖片上傳成功，URL: {imageUrl}");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("未接收到任何檔案");
                    }

                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        ModelState.AddModelError("Image", "請上傳圖片");
                        TempData["Error"] = "請上傳圖片";
                        _logger.LogWarning("圖片 URL 為空，無法儲存");
                        return View(model);
                    }

                    // 取得當前使用者的 ID（而非 UserName）
                    var currentUserId = _userManager.GetUserId(User);
                    _logger.LogInformation($"當前使用者 ID: {currentUserId}");
                    
                    if (string.IsNullOrEmpty(currentUserId))
                    {
                        TempData["Error"] = "無法取得使用者資訊，請重新登入";
                        _logger.LogError("無法取得使用者 ID");
                        return View(model);
                    }
                    
                    // 確保日期是 UTC 格式，避免 PostgreSQL 時區錯誤
                    var utcDate = model.Date.Kind == DateTimeKind.Utc 
                        ? model.Date 
                        : DateTime.SpecifyKind(model.Date, DateTimeKind.Utc);
                    
                    var memory = new Memory
                    {
                        Title = model.Title,
                        Description = model.Description,
                        Date = utcDate, // 使用 UTC 日期
                        ImagePath = imageUrl, // 儲存完整的 URL
                        ApplicationUserId = currentUserId // 使用正確的 User ID
                    };

                    _logger.LogInformation($"準備儲存回憶: {memory.Title}, 圖片: {memory.ImagePath}");
                    _context.Memories.Add(memory);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"回憶儲存成功，ID: {memory.Id}");

                    TempData["Success"] = "回憶已成功新增";
                    return RedirectToAction(nameof(Memories));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "新增回憶時發生錯誤");
                    TempData["Error"] = $"新增失敗: {ex.Message}";
                    return View(model);
                }
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

            // 編輯回憶 (GET)
            public async Task<IActionResult> EditMemory(int id)
            {
                var memory = await _context.Memories
                    .Include(m => m.ApplicationUser)
                    .FirstOrDefaultAsync(m => m.Id == id);
            
                if (memory == null)
                {
                    return NotFound();
                }

                var viewModel = new EditMemoryViewModel
                {
                    Id = memory.Id,
                    Title = memory.Title,
                    Description = memory.Description,
                    Date = memory.Date,
                    ImagePath = memory.ImagePath,
                    UserName = memory.ApplicationUser?.UserName ?? "未知"
                };

                return View(viewModel);
            }

            // 編輯回憶 (POST)
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> EditMemory(EditMemoryViewModel model)
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var memory = await _context.Memories.FindAsync(model.Id);
                if (memory == null)
                {
                    return NotFound();
                }

                memory.Title = model.Title;
                memory.Description = model.Description;
                memory.Date = model.Date;

                await _context.SaveChangesAsync();

                TempData["Success"] = "回憶已成功更新";
                return RedirectToAction(nameof(Memories));
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

                // 刪除圖片檔案（支援雲端或本地）
                if (!string.IsNullOrEmpty(memory.ImagePath))
                {
                    if (_storageService != null && (memory.ImagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase) || memory.ImagePath.Contains("/")))
                    {
                        // 雲端檔案：使用 StorageService 刪除
                        await _storageService.DeleteAsync(memory.ImagePath);
                    }
                    else
                    {
                        // 本地檔案：直接刪除
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", memory.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                }

                _context.Memories.Remove(memory);
                await _context.SaveChangesAsync();

                TempData["Success"] = "回憶已成功刪除";
                return RedirectToAction(nameof(Memories));
            }
        }
}
