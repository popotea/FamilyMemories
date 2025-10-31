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

namespace FamilyMemories.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public MemoriesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/memories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemoryDto>>> GetMemories()
        {
            var memories = await _context.Memories
                .OrderByDescending(m => m.Date)
                .Select(m => new MemoryDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    Date = m.Date,
                    ImagePath = m.ImagePath
                })
                .ToListAsync();

            return Ok(memories);
        }

        // GET: api/memories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MemoryDto>> GetMemory(int id)
        {
            var memory = await _context.Memories.FindAsync(id);

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
                ImagePath = memory.ImagePath
            };

            return Ok(memoryDto);
        }

        // POST: api/memories
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<MemoryDto>> CreateMemory(MemoryCreateDto memoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var memory = new Memory
            {
                Title = memoryDto.Title,
                Description = memoryDto.Description,
                Date = memoryDto.Date,
                ImagePath = memoryDto.ImagePath
            };

            _context.Memories.Add(memory);
            await _context.SaveChangesAsync();

            var resultDto = new MemoryDto
            {
                Id = memory.Id,
                Title = memory.Title,
                Description = memory.Description,
                Date = memory.Date,
                ImagePath = memory.ImagePath
            };

            return CreatedAtAction(nameof(GetMemory), new { id = memory.Id }, resultDto);
        }

        // PUT: api/memories/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateMemory(int id, MemoryUpdateDto memoryDto)
        {
            if (id != memoryDto.Id)
            {
                return BadRequest();
            }

            var memory = await _context.Memories.FindAsync(id);
            if (memory == null)
            {
                return NotFound();
            }

            memory.Title = memoryDto.Title;
            memory.Description = memoryDto.Description;
            memory.Date = memoryDto.Date;

            if (!string.IsNullOrEmpty(memoryDto.ImagePath))
            {
                memory.ImagePath = memoryDto.ImagePath;
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

            return NoContent();
        }

        // DELETE: api/memories/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteMemory(int id)
        {
            var memory = await _context.Memories.FindAsync(id);
            if (memory == null)
            {
                return NotFound();
            }

            _context.Memories.Remove(memory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MemoryExists(int id)
        {
            return _context.Memories.Any(e => e.Id == id);
        }

        // PATCH: api/memories/5
        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> PatchMemory(int id, MemoryPatchDto memoryDto)
        {
            var memory = await _context.Memories.FindAsync(id);
            if (memory == null)
            {
                return NotFound();
            }

            memory.Title = memoryDto.Title;
            memory.Description = memoryDto.Description;

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

            return NoContent();
        }

        // POST: api/memories/batch
        [HttpPost("batch")]
        [Authorize]
        public async Task<IActionResult> BatchUpdate([FromBody] BatchUpdateDto batch)
        {
            if (batch?.Items == null || batch.Items.Count == 0)
                return BadRequest("No items provided.");

            var ids = batch.Items.Select(i => i.Id).ToList();
            var memories = await _context.Memories.Where(m => ids.Contains(m.Id)).ToListAsync();

            foreach (var item in batch.Items)
            {
                var mem = memories.FirstOrDefault(m => m.Id == item.Id);
                if (mem != null)
                {
                    mem.Title = item.Title;
                    mem.Description = item.Description;
                    if (!string.IsNullOrEmpty(item.ImagePath))
                    {
                        mem.ImagePath = item.ImagePath;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/memories/{id}/image
        [HttpPost("{id}/image")]
        [Authorize]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded." });

            // Validate file size (max 5 MB)
            if (file.Length > MaxFileSize)
                return BadRequest(new { error = $"File size exceeds {MaxFileSize / 1024 / 1024} MB limit." });

            // Validate file extension
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                return BadRequest(new { error = "Only image files (.jpg, .jpeg, .png, .gif, .webp) are allowed." });

            var memory = await _context.Memories.FindAsync(id);
            if (memory == null)
                return NotFound();

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // store filename only; front-end will resolve to /uploads/{filename}
            memory.ImagePath = fileName;
            await _context.SaveChangesAsync();

            return Ok(new { fileName, imagePath = memory.ImagePath });
        }
    }

    // DTO Classes
    public class MemoryDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string ImagePath { get; set; }
    }

    public class MemoryCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string ImagePath { get; set; }
    }

    public class MemoryUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string ImagePath { get; set; }
    }

    public class MemoryPatchDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class BatchUpdateDto
    {
        public List<BatchUpdateItem> Items { get; set; }
    }

    public class BatchUpdateItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; } // added to support image updates in batch
    }
}
