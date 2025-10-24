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

namespace FamilyMemories.Pages.Memories
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly FamilyMemories.Data.ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CreateModel(FamilyMemories.Data.ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Memory Memory { get; set; }

        [BindProperty]
        public IFormFile Upload { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Upload != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Upload.FileName);
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Upload.CopyToAsync(stream);
                }
                Memory.ImagePath = fileName;
            }

            _context.Memories.Add(Memory);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
