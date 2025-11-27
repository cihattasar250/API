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
    public class HedefController : ControllerBase
    {
        private readonly SporDbContext _context;
        private readonly ILogger<HedefController> _logger;

        public HedefController(SporDbContext context, ILogger<HedefController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Hedef/health - Health check endpoint
        [HttpGet("health")]
        public ActionResult HealthCheck()
        {
            return Ok(new { 
                status = "OK", 
                message = "Hedef API çalışıyor",
                timestamp = DateTime.Now,
                endpoint = "/api/Hedef"
            });
        }

        // GET: api/Hedef
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Hedef>>> GetHedefler()
        {
            return await _context.Hedefler
                .Include(h => h.Sporcu)
                .Include(h => h.Uye)
                .ToListAsync();
        }

        // GET: api/Hedef/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Hedef>> GetHedef(int id)
        {
            var hedef = await _context.Hedefler
                .Include(h => h.Sporcu)
                .Include(h => h.Uye)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hedef == null)
            {
                return NotFound();
            }

            return hedef;
        }

        // POST: api/Hedef
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Hedef>> PostHedef(Hedef hedef)
        {
            try
            {
                _logger.LogInformation("📥 Hedef kayıt isteği geldi.");
                
                // Model validasyonu
                if (hedef == null)
                {
                    _logger.LogWarning("❌ Hedef bilgileri boş geldi.");
                    return BadRequest(new { message = "Hedef bilgileri boş olamaz." });
                }
                
                _logger.LogInformation("📥 Model: HedefAdi: {HedefAdi}, HedefTarihi: {HedefTarihi}, BaslangicTarihi: {BaslangicTarihi}, Kategori: {Kategori}, HedefDeger: {HedefDeger}, Aciklama: {Aciklama}, Birim: {Birim}", 
                    hedef.HedefAdi, hedef.HedefTarihi, hedef.BaslangicTarihi, hedef.Kategori, hedef.HedefDeger, hedef.Aciklama, hedef.Birim);
                
                // JWT token'dan kullanıcı bilgilerini al
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value;
                var userType = User.FindFirst("UserType")?.Value;

                _logger.LogInformation("🔑 Token'dan alınan bilgiler - UserId: {UserId}, UserType: {UserType}", userId, userType);

                // Kullanıcı ID'si kontrolü
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("❌ Geçersiz token - kullanıcı ID bulunamadı.");
                    return Unauthorized(new { message = "Geçersiz token - kullanıcı ID bulunamadı." });
                }

                // Kullanıcı tipi kontrolü
                if (userType != "Uye")
                {
                    _logger.LogWarning("❌ Sadece üyeler hedef kaydı ekleyebilir. UserType: {UserType}", userType);
                    return Forbid("Sadece üyeler hedef kaydı ekleyebilir.");
                }

                // Kullanıcı ID'sini integer'a çevir
                if (!int.TryParse(userId, out int uyeId))
                {
                    _logger.LogWarning("❌ Geçersiz kullanıcı ID formatı: {UserId}", userId);
                    return BadRequest(new { message = "Geçersiz kullanıcı ID formatı." });
                }

                // Üye var mı kontrol et
                var uyeExists = await _context.Uyeler
                    .Where(u => u.Id == uyeId && u.Aktif)
                    .Select(u => u.Id)
                    .AnyAsync();
                
                if (!uyeExists)
                {
                    _logger.LogWarning("❌ Belirtilen üye bulunamadı veya aktif değil. UyeId: {UyeId}", uyeId);
                    return BadRequest(new { message = "Belirtilen üye bulunamadı veya aktif değil." });
                }

                // Model validasyonu
                if (hedef == null)
                {
                    _logger.LogWarning("❌ Hedef bilgileri boş geldi.");
                    return BadRequest(new { message = "Hedef bilgileri boş olamaz." });
                }

                // HedefAdi kontrolü
                if (string.IsNullOrWhiteSpace(hedef.HedefAdi))
                {
                    _logger.LogWarning("❌ Hedef adı boş.");
                    return BadRequest(new { message = "Hedef adı boş olamaz." });
                }

                // HedefTarihi kontrolü ve parse - eğer default ise bugün + 1 ay
                if (hedef.HedefTarihi == default(DateTime) || hedef.HedefTarihi == DateTime.MinValue || hedef.HedefTarihi == DateTime.MaxValue)
                {
                    _logger.LogWarning("⚠️ Hedef tarihi boş veya geçersiz ({HedefTarihi}), 1 ay sonrası kullanılıyor.", hedef.HedefTarihi);
                    hedef.HedefTarihi = DateTime.Now.AddMonths(1);
                }
                else
                {
                    _logger.LogInformation("✅ HedefTarihi parse edildi: {HedefTarihi}", hedef.HedefTarihi);
                }

                // BaslangicTarihi kontrolü - eğer boşsa bugünü kullan
                if (hedef.BaslangicTarihi == default(DateTime) || hedef.BaslangicTarihi == DateTime.MinValue || hedef.BaslangicTarihi == DateTime.MaxValue)
                {
                    _logger.LogInformation("⚠️ Başlangıç tarihi boş veya geçersiz ({BaslangicTarihi}), bugünün tarihi kullanılıyor.", hedef.BaslangicTarihi);
                    hedef.BaslangicTarihi = DateTime.Now;
                }
                else
                {
                    _logger.LogInformation("✅ BaslangicTarihi parse edildi: {BaslangicTarihi}", hedef.BaslangicTarihi);
                }

                // Üye ID'sini token'dan al
                hedef.UyeId = uyeId;
                hedef.SporcuId = null;
                hedef.Tamamlandi = false;
                
                _logger.LogInformation("💾 Hedef kaydediliyor - HedefAdi: {HedefAdi}, UyeId: {UyeId}, HedefTarihi: {HedefTarihi}, Kategori: {Kategori}, HedefDeger: {HedefDeger}", 
                    hedef.HedefAdi, hedef.UyeId, hedef.HedefTarihi, hedef.Kategori, hedef.HedefDeger);
                
                _context.Hedefler.Add(hedef);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Hedef başarıyla kaydedildi. ID: {Id}, UyeId: {UyeId}", hedef.Id, hedef.UyeId);

                return CreatedAtAction("GetHedef", new { id = hedef.Id }, hedef);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "❌ Veritabanı hatası: {Message}, InnerException: {InnerException}, StackTrace: {StackTrace}", 
                    dbEx.Message, dbEx.InnerException?.Message, dbEx.StackTrace);
                return StatusCode(500, new { 
                    message = "Hedef kaydı eklenirken veritabanı hatası oluştu.", 
                    error = dbEx.Message,
                    innerException = dbEx.InnerException?.Message,
                    stackTrace = dbEx.StackTrace
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Genel hata: {Message}, InnerException: {InnerException}, StackTrace: {StackTrace}", 
                    ex.Message, ex.InnerException?.Message, ex.StackTrace);
                return StatusCode(500, new { 
                    message = "Hedef kaydı eklenirken hata oluştu: " + ex.Message, 
                    error = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // PUT: api/Hedef/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHedef(int id, Hedef hedef)
        {
            if (id != hedef.Id)
            {
                return BadRequest();
            }

            _context.Entry(hedef).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HedefExists(id))
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

        // DELETE: api/Hedef/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHedef(int id)
        {
            var hedef = await _context.Hedefler.FindAsync(id);
            if (hedef == null)
            {
                return NotFound();
            }

            _context.Hedefler.Remove(hedef);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Hedef/Sporcu/5
        [HttpGet("Sporcu/{sporcuId}")]
        public async Task<ActionResult<IEnumerable<Hedef>>> GetHedeflerBySporcu(int sporcuId)
        {
            return await _context.Hedefler
                .Where(h => h.SporcuId == sporcuId)
                .Include(h => h.Sporcu)
                .Include(h => h.Uye)
                .ToListAsync();
        }

        // GET: api/Hedef/Uye/{uyeId}
        [HttpGet("Uye/{uyeId}")]
        public async Task<ActionResult<IEnumerable<Hedef>>> GetHedeflerByUye(int uyeId)
        {
            return await _context.Hedefler
                .Where(h => h.UyeId == uyeId)
                .Include(h => h.Uye)
                .OrderByDescending(h => h.BaslangicTarihi)
                .ToListAsync();
        }

        // GET: api/Hedef/Panel - JWT token'dan üye ID'sini al
        [HttpGet("Panel")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Hedef>>> GetHedeflerPanel()
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

                return await _context.Hedefler
                    .Where(h => h.UyeId == uyeId)
                    .Include(h => h.Uye)
                    .OrderByDescending(h => h.BaslangicTarihi)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hedef verileri alınırken hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Hedef/Aktif/{sporcuId}
        [HttpGet("Aktif/{sporcuId}")]
        public async Task<ActionResult<IEnumerable<Hedef>>> GetAktifHedefler(int sporcuId)
        {
            return await _context.Hedefler
                .Where(h => h.SporcuId == sporcuId && h.Tamamlandi == false)
                .Include(h => h.Sporcu)
                .Include(h => h.Uye)
                .ToListAsync();
        }

        // GET: api/Hedef/Uye/{uyeId}/Aktif
        [HttpGet("Uye/{uyeId}/Aktif")]
        public async Task<ActionResult<IEnumerable<Hedef>>> GetAktifHedeflerByUye(int uyeId)
        {
            return await _context.Hedefler
                .Where(h => h.UyeId == uyeId && h.Tamamlandi == false)
                .Include(h => h.Uye)
                .OrderByDescending(h => h.BaslangicTarihi)
                .ToListAsync();
        }

        // GET: api/Hedef/Tamamlanan/{sporcuId}
        [HttpGet("Tamamlanan/{sporcuId}")]
        public async Task<ActionResult<IEnumerable<Hedef>>> GetTamamlananHedefler(int sporcuId)
        {
            return await _context.Hedefler
                .Where(h => h.SporcuId == sporcuId && h.Tamamlandi == true)
                .Include(h => h.Sporcu)
                .Include(h => h.Uye)
                .ToListAsync();
        }

        // GET: api/Hedef/Uye/{uyeId}/Tamamlanan
        [HttpGet("Uye/{uyeId}/Tamamlanan")]
        public async Task<ActionResult<IEnumerable<Hedef>>> GetTamamlananHedeflerByUye(int uyeId)
        {
            return await _context.Hedefler
                .Where(h => h.UyeId == uyeId && h.Tamamlandi == true)
                .Include(h => h.Uye)
                .OrderByDescending(h => h.TamamlanmaTarihi)
                .ToListAsync();
        }

        // PUT: api/Hedef/Tamamla/5
        [HttpPut("Tamamla/{id}")]
        public async Task<IActionResult> TamamlaHedef(int id)
        {
            var hedef = await _context.Hedefler.FindAsync(id);
            if (hedef == null)
            {
                return NotFound();
            }

            hedef.Tamamlandi = true;
            hedef.TamamlanmaTarihi = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HedefExists(id))
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

        private bool HedefExists(int id)
        {
            return _context.Hedefler.Any(e => e.Id == id);
        }
    }
}
