using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FamilyMemories.Data;
using FamilyMemories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamilyMemories.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MemoriesController(ApplicationDbContext context)
        {
            _context = context;
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
}
