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
    public class AntrenmanController : ControllerBase
    {
        private readonly SporDbContext _context;
        private readonly ILogger<AntrenmanController> _logger;

        public AntrenmanController(SporDbContext context, ILogger<AntrenmanController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Antrenman/health - Health check endpoint
        [HttpGet("health")]
        public ActionResult HealthCheck()
        {
            return Ok(new { 
                status = "OK", 
                message = "Antrenman API çalışıyor",
                timestamp = DateTime.Now,
                endpoint = "/api/Antrenman"
            });
        }

        // GET: api/Antrenman
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Antrenman>>> GetAntrenmanlar()
        {
            return await _context.Antrenmanlar
                .Include(a => a.Sporcu)
                .Include(a => a.Uye)
                .ToListAsync();
        }

        // GET: api/Antrenman/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Antrenman>> GetAntrenman(int id)
        {
            var antrenman = await _context.Antrenmanlar
                .Include(a => a.Sporcu)
                .Include(a => a.Uye)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (antrenman == null)
            {
                return NotFound();
            }

            return antrenman;
        }

        // POST: api/Antrenman
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Antrenman>> PostAntrenman(Antrenman antrenman)
        {
            try
            {
                // Model validasyonu
                if (antrenman == null)
                {
                    _logger.LogWarning("Antrenman bilgileri boş geldi.");
                    return BadRequest(new { message = "Antrenman bilgileri boş olamaz." });
                }

                // AntrenmanAdi kontrolü
                if (string.IsNullOrWhiteSpace(antrenman.AntrenmanAdi))
                {
                    _logger.LogWarning("Antrenman adı boş.");
                    return BadRequest(new { message = "Antrenman adı boş olamaz." });
                }

                // JWT token'dan kullanıcı bilgilerini al
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value;
                var userType = User.FindFirst("UserType")?.Value;

                // Kullanıcı ID'si kontrolü
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Geçersiz token - kullanıcı ID bulunamadı.");
                    return Unauthorized(new { message = "Geçersiz token - kullanıcı ID bulunamadı." });
                }

                // Kullanıcı tipi kontrolü
                if (userType != "Uye")
                {
                    _logger.LogWarning("Sadece üyeler antrenman kaydı ekleyebilir. UserType: {UserType}", userType);
                    return Forbid("Sadece üyeler antrenman kaydı ekleyebilir.");
                }

                // Kullanıcı ID'sini integer'a çevir
                if (!int.TryParse(userId, out int uyeId))
                {
                    _logger.LogWarning("Geçersiz kullanıcı ID formatı: {UserId}", userId);
                    return BadRequest(new { message = "Geçersiz kullanıcı ID formatı." });
                }

                // Üye ID'sini token'dan al
                antrenman.UyeId = uyeId;
                antrenman.SporcuId = null; // Üye ise SporcuId null olmalı

                // Üye var mı kontrol et (sadece ID kontrolü - kolonları yükleme)
                var uyeExists = await _context.Uyeler
                    .Where(u => u.Id == antrenman.UyeId && u.Aktif)
                    .Select(u => u.Id)
                    .AnyAsync();
                
                if (!uyeExists)
                {
                    _logger.LogWarning("Belirtilen üye bulunamadı veya aktif değil. UyeId: {UyeId}", antrenman.UyeId);
                    return BadRequest(new { message = "Belirtilen üye bulunamadı veya aktif değil." });
                }

                // Tarih set et
                antrenman.Tarih = DateTime.Now;
                
                _logger.LogInformation("💾 Antrenman kaydediliyor - AntrenmanAdi: {AntrenmanAdi}, UyeId: {UyeId}", 
                    antrenman.AntrenmanAdi, antrenman.UyeId);
                
                _context.Antrenmanlar.Add(antrenman);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Antrenman başarıyla kaydedildi. ID: {Id}", antrenman.Id);
                
                return CreatedAtAction("GetAntrenman", new { id = antrenman.Id }, antrenman);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "❌ Veritabanı hatası: {Message}, InnerException: {InnerException}", 
                    dbEx.Message, dbEx.InnerException?.Message);
                return StatusCode(500, new { 
                    message = "Antrenman kaydı eklenirken veritabanı hatası oluştu.", 
                    error = dbEx.Message,
                    innerException = dbEx.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Genel hata: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(500, new { 
                    message = "Antrenman kaydı eklenirken hata oluştu.", 
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // PUT: api/Antrenman/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAntrenman(int id, Antrenman antrenman)
        {
            if (id != antrenman.Id)
            {
                return BadRequest();
            }

            _context.Entry(antrenman).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AntrenmanExists(id))
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

        // DELETE: api/Antrenman/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAntrenman(int id)
        {
            var antrenman = await _context.Antrenmanlar.FindAsync(id);
            if (antrenman == null)
            {
                return NotFound();
            }

            _context.Antrenmanlar.Remove(antrenman);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Antrenman/Sporcu/5
        [HttpGet("Sporcu/{sporcuId}")]
        public async Task<ActionResult<IEnumerable<Antrenman>>> GetAntrenmanlarBySporcu(int sporcuId)
        {
            return await _context.Antrenmanlar
                .Where(a => a.SporcuId == sporcuId)
                .Include(a => a.Sporcu)
                .ToListAsync();
        }

        // GET: api/Antrenman/Tarih/{tarih}
        [HttpGet("Tarih/{tarih}")]
        public async Task<ActionResult<IEnumerable<Antrenman>>> GetAntrenmanlarByTarih(DateTime tarih)
        {
            return await _context.Antrenmanlar
                .Where(a => a.Tarih.Date == tarih.Date)
                .Include(a => a.Sporcu)
                .ToListAsync();
        }

        // GET: api/Antrenman/Tip/{tip}
        [HttpGet("Tip/{tip}")]
        public async Task<ActionResult<IEnumerable<Antrenman>>> GetAntrenmanlarByTip(string tip)
        {
            return await _context.Antrenmanlar
                .Where(a => a.AntrenmanTipi == tip)
                .Include(a => a.Sporcu)
                .Include(a => a.Uye)
                .ToListAsync();
        }

        // GET: api/Antrenman/Uye/{uyeId}
        [HttpGet("Uye/{uyeId}")]
        public async Task<ActionResult<IEnumerable<Antrenman>>> GetAntrenmanlarByUye(int uyeId)
        {
            return await _context.Antrenmanlar
                .Where(a => a.UyeId == uyeId)
                .Include(a => a.Uye)
                .OrderByDescending(a => a.Tarih)
                .ToListAsync();
        }

        // GET: api/Antrenman/Panel - JWT token'dan üye ID'sini al
        [HttpGet("Panel")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Antrenman>>> GetAntrenmanlarPanel()
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

                return await _context.Antrenmanlar
                    .Where(a => a.UyeId == uyeId)
                    .Include(a => a.Uye)
                    .OrderByDescending(a => a.Tarih)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Antrenman verileri alınırken hata oluştu.", error = ex.Message });
            }
        }

        private bool AntrenmanExists(int id)
        {
            return _context.Antrenmanlar.Any(e => e.Id == id);
        }
    }
}
