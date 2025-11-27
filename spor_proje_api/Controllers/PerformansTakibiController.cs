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
    public class PerformansTakibiController : ControllerBase
    {
        private readonly SporDbContext _context;

        public PerformansTakibiController(SporDbContext context)
        {
            _context = context;
        }

        // GET: api/PerformansTakibi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PerformansTakibi>>> GetPerformansTakibi()
        {
            try
            {
                return await _context.PerformansTakibi
                    .Include(p => p.Uye)
                    .OrderByDescending(p => p.Tarih)
                    .ToListAsync();
            }
            catch (Exception sqlEx) when (sqlEx.Message.Contains("Invalid column name"))
            {
                // Paket kolonları yoksa Include olmadan getir
                return await _context.PerformansTakibi
                    .OrderByDescending(p => p.Tarih)
                    .ToListAsync();
            }
        }

        // GET: api/PerformansTakibi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PerformansTakibi>> GetPerformansTakibi(int id)
        {
            try
            {
                var performansTakibi = await _context.PerformansTakibi
                    .Include(p => p.Uye)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (performansTakibi == null)
                {
                    return NotFound();
                }

                return performansTakibi;
            }
            catch (Exception sqlEx) when (sqlEx.Message.Contains("Invalid column name"))
            {
                // Paket kolonları yoksa Include olmadan getir
                var performansTakibi = await _context.PerformansTakibi
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (performansTakibi == null)
                {
                    return NotFound();
                }

                return performansTakibi;
            }
        }

        // GET: api/PerformansTakibi/Uye/5
        [HttpGet("Uye/{uyeId}")]
        public async Task<ActionResult<IEnumerable<PerformansTakibi>>> GetPerformansTakibiByUye(int uyeId)
        {
            try
            {
                return await _context.PerformansTakibi
                    .Where(p => p.UyeId == uyeId)
                    .Include(p => p.Uye)
                    .OrderByDescending(p => p.Tarih)
                    .ToListAsync();
            }
            catch (Exception sqlEx) when (sqlEx.Message.Contains("Invalid column name"))
            {
                // Paket kolonları yoksa Include olmadan getir
                return await _context.PerformansTakibi
                    .Where(p => p.UyeId == uyeId)
                    .OrderByDescending(p => p.Tarih)
                    .ToListAsync();
            }
        }

        // GET: api/PerformansTakibi/Uye/5/SonKayit
        [HttpGet("Uye/{uyeId}/SonKayit")]
        public async Task<ActionResult<PerformansTakibi>> GetSonPerformansTakibiByUye(int uyeId)
        {
            try
            {
                var sonKayit = await _context.PerformansTakibi
                    .Where(p => p.UyeId == uyeId)
                    .Include(p => p.Uye)
                    .OrderByDescending(p => p.Tarih)
                    .FirstOrDefaultAsync();

                if (sonKayit == null)
                {
                    return NotFound();
                }

                return sonKayit;
            }
            catch (Exception sqlEx) when (sqlEx.Message.Contains("Invalid column name"))
            {
                // Paket kolonları yoksa Include olmadan getir
                var sonKayit = await _context.PerformansTakibi
                    .Where(p => p.UyeId == uyeId)
                    .OrderByDescending(p => p.Tarih)
                    .FirstOrDefaultAsync();

                if (sonKayit == null)
                {
                    return NotFound();
                }

                return sonKayit;
            }
        }

        // POST: api/PerformansTakibi
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<PerformansTakibi>> PostPerformansTakibi(PerformansTakibi performansTakibi)
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
                    return Forbid("Sadece üyeler performans kaydı ekleyebilir.");
                }

                // Kullanıcı ID'sini integer'a çevir
                if (!int.TryParse(userId, out int uyeId))
                {
                    return BadRequest(new { message = "Geçersiz kullanıcı ID formatı." });
                }

                // Üye ID'sini token'dan al
                performansTakibi.UyeId = uyeId;

                // Üye var mı kontrol et (sadece ID kontrolü - kolonları yükleme)
                var uyeExists = await _context.Uyeler
                    .Where(u => u.Id == performansTakibi.UyeId && u.Aktif)
                    .Select(u => u.Id)
                    .AnyAsync();
                
                if (!uyeExists)
                {
                    return BadRequest("Belirtilen üye bulunamadı veya aktif değil.");
                }

                // Aynı tarihte kayıt kontrolü kaldırıldı - kullanıcı istediği kadar kayıt ekleyebilir

                performansTakibi.KayitTarihi = DateTime.Now;
                _context.PerformansTakibi.Add(performansTakibi);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetPerformansTakibi", new { id = performansTakibi.Id }, performansTakibi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Performans kaydı eklenirken hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/PerformansTakibi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerformansTakibi(int id, PerformansTakibi performansTakibi)
        {
            if (id != performansTakibi.Id)
            {
                return BadRequest();
            }

            // Üye var mı kontrol et (sadece ID kontrolü - kolonları yükleme)
            var uyeExists = await _context.Uyeler
                .Where(u => u.Id == performansTakibi.UyeId && u.Aktif)
                .Select(u => u.Id)
                .AnyAsync();
            
            if (!uyeExists)
            {
                return BadRequest("Belirtilen üye bulunamadı veya aktif değil.");
            }

            // Aynı tarihte kayıt kontrolü kaldırıldı - kullanıcı istediği kadar kayıt ekleyebilir

            _context.Entry(performansTakibi).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PerformansTakibiExists(id))
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

        // DELETE: api/PerformansTakibi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerformansTakibi(int id)
        {
            var performansTakibi = await _context.PerformansTakibi.FindAsync(id);
            if (performansTakibi == null)
            {
                return NotFound();
            }

            _context.PerformansTakibi.Remove(performansTakibi);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/PerformansTakibi/Uye/5/Istatistikler
        [HttpGet("Uye/{uyeId}/Istatistikler")]
        public async Task<ActionResult<object>> GetPerformansIstatistikleri(int uyeId)
        {
            var kayitlar = await _context.PerformansTakibi
                .Where(p => p.UyeId == uyeId)
                .OrderBy(p => p.Tarih)
                .ToListAsync();

            if (!kayitlar.Any())
            {
                return NotFound("Bu üye için performans kaydı bulunamadı.");
            }

            var ilkKayit = kayitlar.First();
            var sonKayit = kayitlar.Last();

            var istatistikler = new
            {
                ToplamKayitSayisi = kayitlar.Count,
                IlkKayitTarihi = ilkKayit.Tarih,
                SonKayitTarihi = sonKayit.Tarih,
                KiloDegisimi = sonKayit.Kilo - ilkKayit.Kilo,
                EnYuksekKilo = kayitlar.Max(k => k.Kilo),
                EnDusukKilo = kayitlar.Min(k => k.Kilo),
                OrtalamaKilo = kayitlar.Average(k => k.Kilo),
                ToplamKardiyoSuresi = kayitlar.Where(k => k.KardiyoSuresi.HasValue).Sum(k => k.KardiyoSuresi!.Value),
                OrtalamaKardiyoSuresi = kayitlar.Where(k => k.KardiyoSuresi.HasValue).Average(k => k.KardiyoSuresi!.Value),
                VucutYagOraniDegisimi = sonKayit.VucutYagOrani.HasValue && ilkKayit.VucutYagOrani.HasValue 
                    ? sonKayit.VucutYagOrani - ilkKayit.VucutYagOrani 
                    : (decimal?)null,
                KasKutlesiDegisimi = sonKayit.KasKutlesi.HasValue && ilkKayit.KasKutlesi.HasValue 
                    ? sonKayit.KasKutlesi - ilkKayit.KasKutlesi 
                    : (decimal?)null
            };

            return Ok(istatistikler);
        }

        // GET: api/PerformansTakibi/Panel - JWT token'dan üye ID'sini al
        [HttpGet("Panel")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PerformansTakibi>>> GetPerformansTakibiPanel()
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

                try
                {
                    return await _context.PerformansTakibi
                        .Where(p => p.UyeId == uyeId)
                        .Include(p => p.Uye)
                        .OrderByDescending(p => p.Tarih)
                        .ToListAsync();
                }
                catch (Exception sqlEx) when (sqlEx.Message.Contains("Invalid column name"))
                {
                    // Paket kolonları yoksa Include olmadan getir
                    return await _context.PerformansTakibi
                        .Where(p => p.UyeId == uyeId)
                        .OrderByDescending(p => p.Tarih)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Performans takibi verileri alınırken hata oluştu.", error = ex.Message });
            }
        }

        private bool PerformansTakibiExists(int id)
        {
            return _context.PerformansTakibi.Any(e => e.Id == id);
        }
    }
}
