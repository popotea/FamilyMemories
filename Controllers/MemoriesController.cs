using System;
using System.IO;
using System.Threading.Tasks;
using FamilyMemories.Data;
using FamilyMemories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamilyMemories.Controllers
{
    public class MemoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MemoriesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Index uses client-side API to load items, keep view simple
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Memory memory, IFormFile Upload)
        {
            if (!ModelState.IsValid)
            {
                return View(memory);
            }

            if (Upload != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Upload.FileName);
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Upload.CopyToAsync(stream);
                }
                memory.ImagePath = fileName;
            }

            _context.Memories.Add(memory);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var memory = await _context.Memories.FirstOrDefaultAsync(m => m.Id == id);
            if (memory == null) return NotFound();

            return View(memory);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Memory memory, IFormFile Upload)
        {
            if (!ModelState.IsValid) return View(memory);

            var memoryToUpdate = await _context.Memories.FindAsync(memory.Id);
            if (memoryToUpdate == null) return NotFound();

            memoryToUpdate.Title = memory.Title;
            memoryToUpdate.Description = memory.Description;
            memoryToUpdate.Date = memory.Date;

            if (Upload != null)
            {
                if (!string.IsNullOrEmpty(memoryToUpdate.ImagePath))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, "uploads", memoryToUpdate.ImagePath);
                    if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Upload.FileName);
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Upload.CopyToAsync(stream);
                }
                memoryToUpdate.ImagePath = fileName;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var memory = await _context.Memories.FirstOrDefaultAsync(m => m.Id == id);
            if (memory == null) return NotFound();

            return View(memory);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var memory = await _context.Memories.FindAsync(id);
            if (memory != null)
            {
                if (!string.IsNullOrEmpty(memory.ImagePath))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, "uploads", memory.ImagePath);
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }

                _context.Memories.Remove(memory);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
