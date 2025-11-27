using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using spor_proje_api.Data;
using spor_proje_api.Models;

namespace spor_proje_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SporcuController : ControllerBase
    {
        private readonly SporDbContext _context;

        public SporcuController(SporDbContext context)
        {
            _context = context;
        }

        // GET: api/Sporcu
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sporcu>>> GetSporcular()
        {
            return await _context.Sporcular
                .Include(s => s.Antrenmanlar)
                .Include(s => s.Performanslar)
                .Include(s => s.BeslenmeKayitlari)
                .Include(s => s.Hedefler)
                .ToListAsync();
        }

        // GET: api/Sporcu/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Sporcu>> GetSporcu(int id)
        {
            var sporcu = await _context.Sporcular
                .Include(s => s.Antrenmanlar)
                .Include(s => s.Performanslar)
                .Include(s => s.BeslenmeKayitlari)
                .Include(s => s.Hedefler)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sporcu == null)
            {
                return NotFound();
            }

            return sporcu;
        }

        // POST: api/Sporcu
        [HttpPost]
        public async Task<ActionResult<Sporcu>> PostSporcu(Sporcu sporcu)
        {
            sporcu.KayitTarihi = DateTime.Now;
            _context.Sporcular.Add(sporcu);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSporcu", new { id = sporcu.Id }, sporcu);
        }

        // PUT: api/Sporcu/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSporcu(int id, Sporcu sporcu)
        {
            if (id != sporcu.Id)
            {
                return BadRequest();
            }

            _context.Entry(sporcu).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SporcuExists(id))
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

        // DELETE: api/Sporcu/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSporcu(int id)
        {
            var sporcu = await _context.Sporcular.FindAsync(id);
            if (sporcu == null)
            {
                return NotFound();
            }

            _context.Sporcular.Remove(sporcu);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Sporcu/Aktif
        [HttpGet("Aktif")]
        public async Task<ActionResult<IEnumerable<Sporcu>>> GetAktifSporcular()
        {
            return await _context.Sporcular
                .Where(s => s.Aktif == true)
                .ToListAsync();
        }

        // GET: api/Sporcu/SporDali/{sporDali}
        [HttpGet("SporDali/{sporDali}")]
        public async Task<ActionResult<IEnumerable<Sporcu>>> GetSporcularBySporDali(string sporDali)
        {
            return await _context.Sporcular
                .Where(s => s.SporDali == sporDali)
                .ToListAsync();
        }

        private bool SporcuExists(int id)
        {
            return _context.Sporcular.Any(e => e.Id == id);
        }
    }
}
