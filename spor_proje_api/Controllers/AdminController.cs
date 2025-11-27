using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using spor_proje_api.Data;
using spor_proje_api.Models;
using spor_proje_api.Services;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace spor_proje_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly SporDbContext _context;
        private readonly JwtTokenService _jwtTokenService;

        public AdminController(SporDbContext context, JwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        /// <summary>
        /// Test endpoint
        /// </summary>
        [HttpGet("test")]
        public ActionResult Test()
        {
            return Ok(new { message = "API çalışıyor!", timestamp = DateTime.Now });
        }

        /// <summary>
        /// Yeni admin hesabı oluşturur
        /// </summary>
        /// <param name="adminDto">Admin kayıt bilgileri</param>
        /// <returns>Oluşturulan admin bilgileri</returns>
        [HttpPost("kayit")]
        public async Task<ActionResult<Admin>> AdminKayit([FromBody] AdminKayitDto adminDto)
        {
            try
            {
                // Admin numarası benzersizlik kontrolü
                if (await _context.Adminler.AnyAsync(a => a.AdminNumarasi == adminDto.AdminNumarasi))
                {
                    return BadRequest(new { message = "Bu admin numarası zaten kullanılıyor." });
                }

                // Email benzersizlik kontrolü
                if (await _context.Adminler.AnyAsync(a => a.Email == adminDto.Email))
                {
                    return BadRequest(new { message = "Bu email adresi zaten kullanılıyor." });
                }

                // Şifre hash'leme
                var hashedPassword = HashPassword(adminDto.Sifre);

                var admin = new Admin
                {
                    Ad = adminDto.Ad,
                    Soyad = adminDto.Soyad,
                    Email = adminDto.Email,
                    Telefon = adminDto.Telefon,
                    AdminNumarasi = adminDto.AdminNumarasi,
                    Sifre = hashedPassword,
                    KayitTarihi = DateTime.Now,
                    Aktif = true
                };

                _context.Adminler.Add(admin);
                
                // Veritabanına kaydet
                var rowsAffected = await _context.SaveChangesAsync();
                
                // Kayıt sonrası admin ID'sini al
                var savedAdmin = await _context.Adminler
                    .FirstOrDefaultAsync(a => a.AdminNumarasi == adminDto.AdminNumarasi);

                if (savedAdmin == null)
                {
                    return StatusCode(500, new { 
                        message = "Admin kayıt edildi ancak veritabanından okunamadı.", 
                        rowsAffected = rowsAffected 
                    });
                }

                // Şifreyi response'dan çıkar
                savedAdmin.Sifre = string.Empty;

                return CreatedAtAction(nameof(AdminKayit), new { id = savedAdmin.Id }, new
                {
                    id = savedAdmin.Id,
                    ad = savedAdmin.Ad,
                    soyad = savedAdmin.Soyad,
                    email = savedAdmin.Email,
                    telefon = savedAdmin.Telefon,
                    adminNumarasi = savedAdmin.AdminNumarasi,
                    kayitTarihi = savedAdmin.KayitTarihi,
                    aktif = savedAdmin.Aktif,
                    message = "Admin başarıyla kaydedildi! (ID: " + savedAdmin.Id + ")"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Admin kaydı sırasında hata oluştu.", 
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Admin girişi yapar
        /// </summary>
        /// <param name="loginDto">Giriş bilgileri</param>
        /// <returns>Giriş sonucu ve admin bilgileri</returns>
        [HttpPost("giris")]
        public async Task<ActionResult<AdminGirisResponseDto>> AdminGiris([FromBody] AdminGirisDto loginDto)
        {
            try
            {
                // Giriş bilgileri kontrolü
                if (string.IsNullOrEmpty(loginDto.AdminNumarasi))
                {
                    return BadRequest(new { message = "Admin numarası boş olamaz." });
                }

                if (string.IsNullOrEmpty(loginDto.Sifre))
                {
                    return BadRequest(new { message = "Şifre boş olamaz." });
                }

                // Admin numarası ile admin bul
                var admin = await _context.Adminler
                    .FirstOrDefaultAsync(a => a.AdminNumarasi == loginDto.AdminNumarasi);

                if (admin == null)
                {
                    return Unauthorized(new { message = "Geçersiz admin numarası." });
                }

                // Admin aktif mi kontrol et
                if (!admin.Aktif)
                {
                    return Unauthorized(new { message = "Bu admin hesabı aktif değil." });
                }

                // Şifre kontrolü
                if (!VerifyPassword(loginDto.Sifre, admin.Sifre))
                {
                    return Unauthorized(new { message = "Geçersiz şifre." });
                }

                // Son giriş tarihini güncelle
                admin.SonGirisTarihi = DateTime.Now;
                await _context.SaveChangesAsync();

                // JWT Token oluştur
                var token = _jwtTokenService.GenerateToken(admin.Id.ToString(), "Admin", admin.Ad + " " + admin.Soyad);

                // Token kontrolü
                if (string.IsNullOrEmpty(token))
                {
                    return StatusCode(500, new { message = "Token oluşturulamadı. Lütfen tekrar deneyin." });
                }

                var response = new AdminGirisResponseDto
                {
                    Id = admin.Id,
                    Ad = admin.Ad,
                    Soyad = admin.Soyad,
                    Email = admin.Email,
                    Telefon = admin.Telefon,
                    AdminNumarasi = admin.AdminNumarasi,
                    KayitTarihi = admin.KayitTarihi,
                    SonGirisTarihi = admin.SonGirisTarihi,
                    Token = token,
                    Message = "Giriş başarılı"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Giriş sırasında hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Tüm adminleri listeler
        /// </summary>
        /// <returns>Admin listesi</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Admin>>> GetAdminler()
        {
            var adminler = await _context.Adminler
                .Where(a => a.Aktif)
                .Select(a => new Admin
                {
                    Id = a.Id,
                    Ad = a.Ad,
                    Soyad = a.Soyad,
                    Email = a.Email,
                    Telefon = a.Telefon,
                    AdminNumarasi = a.AdminNumarasi,
                    KayitTarihi = a.KayitTarihi,
                    SonGirisTarihi = a.SonGirisTarihi,
                    Aktif = a.Aktif
                })
                .ToListAsync();

            return Ok(adminler);
        }

        /// <summary>
        /// ID'ye göre admin getirir
        /// </summary>
        /// <param name="id">Admin ID</param>
        /// <returns>Admin bilgileri</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Admin>> GetAdmin(int id)
        {
            var admin = await _context.Adminler
                .Where(a => a.Id == id && a.Aktif)
                .Select(a => new Admin
                {
                    Id = a.Id,
                    Ad = a.Ad,
                    Soyad = a.Soyad,
                    Email = a.Email,
                    Telefon = a.Telefon,
                    AdminNumarasi = a.AdminNumarasi,
                    KayitTarihi = a.KayitTarihi,
                    SonGirisTarihi = a.SonGirisTarihi,
                    Aktif = a.Aktif
                })
                .FirstOrDefaultAsync();

            if (admin == null)
            {
                return NotFound(new { message = "Admin bulunamadı." });
            }

            return Ok(admin);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }

        /// <summary>
        /// Admin paneli - sadece adminler erişebilir
        /// </summary>
        /// <returns>Admin panel verileri</returns>
        [HttpGet("panel")]
        [Authorize]
        public async Task<ActionResult> AdminPanel()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userType = User.FindFirst("UserType")?.Value;

                if (userType != "Admin")
                {
                    return Forbid("Sadece adminler bu sayfaya erişebilir.");
                }

                if (!int.TryParse(userId, out int adminId))
                {
                    return BadRequest(new { message = "Geçersiz admin ID formatı." });
                }

                var admin = await _context.Adminler.FindAsync(adminId);
                if (admin == null)
                {
                    return NotFound(new { message = "Admin bulunamadı." });
                }

                var adminCount = await _context.Adminler.CountAsync();
                var uyeCount = await _context.Uyeler.CountAsync();
                var sporcuCount = await _context.Sporcular.CountAsync();

                return Ok(new
                {
                    admin = new
                    {
                        Id = admin.Id,
                        Ad = admin.Ad,
                        Soyad = admin.Soyad,
                        Email = admin.Email,
                        Telefon = admin.Telefon,
                        AdminNumarasi = admin.AdminNumarasi,
                        KayitTarihi = admin.KayitTarihi,
                        SonGirisTarihi = admin.SonGirisTarihi
                    },
                    istatistikler = new
                    {
                        adminSayisi = adminCount,
                        uyeSayisi = uyeCount,
                        sporcuSayisi = sporcuCount
                    },
                    message = "Admin paneline hoş geldiniz!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Admin panel verileri alınırken hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Veritabanı bağlantısını test eder
        /// </summary>
        /// <returns>Veritabanı durumu</returns>
        [HttpGet("test-db")]
        public async Task<ActionResult> TestDatabase()
        {
            try
            {
                var adminCount = await _context.Adminler.CountAsync();
                var uyeCount = await _context.Uyeler.CountAsync();
                var sporcuCount = await _context.Sporcular.CountAsync();
                
                return Ok(new { 
                    message = "Veritabanı bağlantısı başarılı!",
                    adminCount = adminCount,
                    uyeCount = uyeCount,
                    sporcuCount = sporcuCount,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Veritabanı bağlantı hatası!",
                    error = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }
    }

    // DTO sınıfları
    public class AdminKayitDto
    {
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public string AdminNumarasi { get; set; } = string.Empty;
        public string Sifre { get; set; } = string.Empty;
    }

    public class AdminGirisDto
    {
        public string AdminNumarasi { get; set; } = string.Empty;
        public string Sifre { get; set; } = string.Empty;
    }

    public class AdminGirisResponseDto
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public string AdminNumarasi { get; set; } = string.Empty;
        public DateTime KayitTarihi { get; set; }
        public DateTime? SonGirisTarihi { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
        
        public string Message { get; set; } = string.Empty;
    }
}
