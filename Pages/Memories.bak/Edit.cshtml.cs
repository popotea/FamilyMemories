using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using FamilyMemories.Data;
using FamilyMemories.Models;
using Microsoft.EntityFrameworkCore;

namespace FamilyMemories.Pages.Memories
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly FamilyMemories.Data.ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public EditModel(FamilyMemories.Data.ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public Memory Memory { get; set; }

        [BindProperty]
        public IFormFile Upload { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Memory = await _context.Memories.FirstOrDefaultAsync(m => m.Id == id);

            if (Memory == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var memoryToUpdate = await _context.Memories.FindAsync(Memory.Id);
            if (memoryToUpdate == null)
            {
                return NotFound();
            }

            // Update the properties
            memoryToUpdate.Title = Memory.Title;
            memoryToUpdate.Description = Memory.Description;
            memoryToUpdate.Date = Memory.Date;

            // Handle image upload if a new file is provided
            if (Upload != null)
            {
                // Delete old image file if it exists
                if (!string.IsNullOrEmpty(memoryToUpdate.ImagePath))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, "uploads", memoryToUpdate.ImagePath);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Save new image file
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Upload.FileName);
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Upload.CopyToAsync(stream);
                }
                memoryToUpdate.ImagePath = fileName;
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
