using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using spor_proje_api.Data;
using spor_proje_api.Models;

namespace spor_proje_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PerformansController : ControllerBase
    {
        private readonly SporDbContext _context;

        public PerformansController(SporDbContext context)
        {
            _context = context;
        }

        // GET: api/Performans
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Performans>>> GetPerformanslar()
        {
            return await _context.Performanslar
                .Include(p => p.Sporcu)
                .ToListAsync();
        }

        // GET: api/Performans/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Performans>> GetPerformans(int id)
        {
            var performans = await _context.Performanslar
                .Include(p => p.Sporcu)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (performans == null)
            {
                return NotFound();
            }

            return performans;
        }

        // POST: api/Performans
        [HttpPost]
        public async Task<ActionResult<Performans>> PostPerformans(Performans performans)
        {
            performans.Tarih = DateTime.Now;
            _context.Performanslar.Add(performans);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPerformans", new { id = performans.Id }, performans);
        }

        // PUT: api/Performans/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerformans(int id, Performans performans)
        {
            if (id != performans.Id)
            {
                return BadRequest();
            }

            _context.Entry(performans).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PerformansExists(id))
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

        // DELETE: api/Performans/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerformans(int id)
        {
            var performans = await _context.Performanslar.FindAsync(id);
            if (performans == null)
            {
                return NotFound();
            }

            _context.Performanslar.Remove(performans);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Performans/Sporcu/5
        [HttpGet("Sporcu/{sporcuId}")]
        public async Task<ActionResult<IEnumerable<Performans>>> GetPerformanslarBySporcu(int sporcuId)
        {
            return await _context.Performanslar
                .Where(p => p.SporcuId == sporcuId)
                .Include(p => p.Sporcu)
                .ToListAsync();
        }

        // GET: api/Performans/Kategori/{kategori}
        [HttpGet("Kategori/{kategori}")]
        public async Task<ActionResult<IEnumerable<Performans>>> GetPerformanslarByKategori(string kategori)
        {
            return await _context.Performanslar
                .Where(p => p.Kategori == kategori)
                .Include(p => p.Sporcu)
                .ToListAsync();
        }

        // GET: api/Performans/EnIyi/{sporcuId}
        [HttpGet("EnIyi/{sporcuId}")]
        public async Task<ActionResult<IEnumerable<Performans>>> GetEnIyiPerformanslar(int sporcuId)
        {
            return await _context.Performanslar
                .Where(p => p.SporcuId == sporcuId)
                .OrderByDescending(p => p.Deger)
                .Take(10)
                .Include(p => p.Sporcu)
                .ToListAsync();
        }

        private bool PerformansExists(int id)
        {
            return _context.Performanslar.Any(e => e.Id == id);
        }
    }
}
