using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using FamilyMemories.Data;
using FamilyMemories.Models;
using System.IO;

namespace FamilyMemories.Pages.Memories
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly FamilyMemories.Data.ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DeleteModel(FamilyMemories.Data.ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public Memory Memory { get; set; }

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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Memory = await _context.Memories.FindAsync(id);

            if (Memory != null)
            {
                // Delete the associated image file if it exists
                if (!string.IsNullOrEmpty(Memory.ImagePath))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, "uploads", Memory.ImagePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Memories.Remove(Memory);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
