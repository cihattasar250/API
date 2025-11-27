using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using spor_proje_api.Data;
using spor_proje_api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace spor_proje_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BeslenmeController : ControllerBase
    {
        private readonly SporDbContext _context;

        public BeslenmeController(SporDbContext context)
        {
            _context = context;
        }

        // GET: api/Beslenme
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Beslenme>>> GetBeslenmeKayitlari()
        {
            return await _context.BeslenmeKayitlari
                .Include(b => b.Sporcu)
                .Include(b => b.Uye)
                .ToListAsync();
        }

        // GET: api/Beslenme/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Beslenme>> GetBeslenme(int id)
        {
            var beslenme = await _context.BeslenmeKayitlari
                .Include(b => b.Sporcu)
                .Include(b => b.Uye)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (beslenme == null)
            {
                return NotFound();
            }

            return beslenme;
        }

        // POST: api/Beslenme
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Beslenme>> PostBeslenme(Beslenme beslenme)
        {
            try
            {
                // Model validasyonu
                if (beslenme == null)
                {
                    return BadRequest(new { message = "Beslenme bilgileri boş olamaz." });
                }

                // JWT token'dan kullanıcı bilgilerini al
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value;
                var userType = User.FindFirst("UserType")?.Value;

                // Kullanıcı ID'si kontrolü
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Geçersiz token - kullanıcı ID bulunamadı." });
                }

                // Kullanıcı tipi kontrolü
                if (userType != "Uye")
                {
                    return Forbid("Sadece üyeler beslenme kaydı ekleyebilir.");
                }

                // Kullanıcı ID'sini integer'a çevir
                if (!int.TryParse(userId, out int uyeId))
                {
                    return BadRequest(new { message = "Geçersiz kullanıcı ID formatı." });
                }

                // Üye ID'sini token'dan al
                beslenme.UyeId = uyeId;
                beslenme.SporcuId = null; // Üye ise SporcuId null olmalı

                // Üye var mı kontrol et (sadece ID kontrolü - kolonları yükleme)
                var uyeExists = await _context.Uyeler
                    .Where(u => u.Id == beslenme.UyeId && u.Aktif)
                    .Select(u => u.Id)
                    .AnyAsync();
                
                if (!uyeExists)
                {
                    return BadRequest(new { message = "Belirtilen üye bulunamadı veya aktif değil." });
                }

                beslenme.Tarih = DateTime.Now;
                _context.BeslenmeKayitlari.Add(beslenme);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetBeslenme", new { id = beslenme.Id }, beslenme);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Beslenme kaydı eklenirken hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/Beslenme/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBeslenme(int id, Beslenme beslenme)
        {
            if (id != beslenme.Id)
            {
                return BadRequest();
            }

            _context.Entry(beslenme).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BeslenmeExists(id))
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

        // DELETE: api/Beslenme/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBeslenme(int id)
        {
            var beslenme = await _context.BeslenmeKayitlari.FindAsync(id);
            if (beslenme == null)
            {
                return NotFound();
            }

            _context.BeslenmeKayitlari.Remove(beslenme);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Beslenme/Sporcu/5
        [HttpGet("Sporcu/{sporcuId}")]
        public async Task<ActionResult<IEnumerable<Beslenme>>> GetBeslenmeBySporcu(int sporcuId)
        {
            return await _context.BeslenmeKayitlari
                .Where(b => b.SporcuId == sporcuId)
                .Include(b => b.Sporcu)
                .ToListAsync();
        }

        // GET: api/Beslenme/Ogun/{ogun}
        [HttpGet("Ogun/{ogun}")]
        public async Task<ActionResult<IEnumerable<Beslenme>>> GetBeslenmeByOgun(string ogun)
        {
            return await _context.BeslenmeKayitlari
                .Where(b => b.Ogun == ogun)
                .Include(b => b.Sporcu)
                .Include(b => b.Uye)
                .ToListAsync();
        }

        // GET: api/Beslenme/Gunluk/{sporcuId}/{tarih}
        [HttpGet("Gunluk/{sporcuId}/{tarih}")]
        public async Task<ActionResult<IEnumerable<Beslenme>>> GetGunlukBeslenme(int sporcuId, DateTime tarih)
        {
            return await _context.BeslenmeKayitlari
                .Where(b => b.SporcuId == sporcuId && b.Tarih.Date == tarih.Date)
                .Include(b => b.Sporcu)
                .Include(b => b.Uye)
                .ToListAsync();
        }

        // GET: api/Beslenme/Uye/{uyeId}
        [HttpGet("Uye/{uyeId}")]
        public async Task<ActionResult<IEnumerable<Beslenme>>> GetBeslenmeByUye(int uyeId)
        {
            return await _context.BeslenmeKayitlari
                .Where(b => b.UyeId == uyeId)
                .Include(b => b.Uye)
                .OrderByDescending(b => b.Tarih)
                .ToListAsync();
        }

        // GET: api/Beslenme/Panel - JWT token'dan üye ID'sini al
        [HttpGet("Panel")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Beslenme>>> GetBeslenmePanel()
        {
            try
            {
                // JWT token'dan kullanıcı bilgilerini al
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value;
                var userType = User.FindFirst("UserType")?.Value;

                // Kullanıcı ID'si kontrolü
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Geçersiz token - kullanıcı ID bulunamadı." });
                }

                // Kullanıcı tipi kontrolü
                if (userType != "Uye")
                {
                    return Forbid("Sadece üyeler bu sayfaya erişebilir.");
                }

                // Kullanıcı ID'sini integer'a çevir
                if (!int.TryParse(userId, out int uyeId))
                {
                    return BadRequest(new { message = "Geçersiz kullanıcı ID formatı." });
                }

                return await _context.BeslenmeKayitlari
                    .Where(b => b.UyeId == uyeId)
                    .Include(b => b.Uye)
                    .OrderByDescending(b => b.Tarih)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Beslenme verileri alınırken hata oluştu.", error = ex.Message });
            }
        }

        private bool BeslenmeExists(int id)
        {
            return _context.BeslenmeKayitlari.Any(e => e.Id == id);
        }
    }
}
