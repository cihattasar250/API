using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using spor_proje_api.Data;
using spor_proje_api.Models;
using spor_proje_api.Services;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using System.Text.Json.Serialization;

namespace spor_proje_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UyeController : ControllerBase
    {
        private readonly SporDbContext _context;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ILogger<UyeController> _logger;

        public UyeController(SporDbContext context, JwtTokenService jwtTokenService, ILogger<UyeController> logger)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        /// <summary>
        /// Yeni üye kaydı oluşturur
        /// </summary>
        /// <param name="uyeDto">Üye kayıt bilgileri</param>
        /// <returns>Oluşturulan üye bilgileri</returns>
        [HttpPost("kayit")]
        public async Task<ActionResult<Uye>> UyeKayit([FromBody] UyeKayitDto uyeDto)
        {
            try
            {
                // Validasyon kontrolü
                if (string.IsNullOrWhiteSpace(uyeDto.Ad))
                {
                    return BadRequest(new { message = "Ad boş olamaz." });
                }

                if (string.IsNullOrWhiteSpace(uyeDto.UyeNumarasi))
                {
                    return BadRequest(new { message = "Üye numarası boş olamaz." });
                }

                // Üye numarası benzersizlik kontrolü
                if (await _context.Uyeler.AnyAsync(u => u.UyeNumarasi == uyeDto.UyeNumarasi))
                {
                    return BadRequest(new { message = "Bu üye numarası zaten kullanılıyor." });
                }

                // Email benzersizlik kontrolü
                if (!string.IsNullOrWhiteSpace(uyeDto.Email) && await _context.Uyeler.AnyAsync(u => u.Email == uyeDto.Email))
                {
                    return BadRequest(new { message = "Bu email adresi zaten kullanılıyor." });
                }

                // Şifre hash'leme
                var hashedPassword = HashPassword(uyeDto.Sifre);

                var uye = new Uye
                {
                    Ad = uyeDto.Ad,
                    Soyad = uyeDto.Soyad,
                    Email = uyeDto.Email,
                    Telefon = uyeDto.Telefon,
                    DogumTarihi = uyeDto.DogumTarihi,
                    Cinsiyet = uyeDto.Cinsiyet,
                    Adres = uyeDto.Adres,
                    UyeNumarasi = uyeDto.UyeNumarasi,
                    Sifre = hashedPassword,
                    KayitTarihi = DateTime.Now,
                    Aktif = true
                };

                _context.Uyeler.Add(uye);
                
                // Veritabanına kaydet
                var rowsAffected = await _context.SaveChangesAsync();
                
                // Kayıt sonrası üye ID'sini al
                var savedUye = await _context.Uyeler
                    .FirstOrDefaultAsync(u => u.UyeNumarasi == uyeDto.UyeNumarasi);

                if (savedUye == null)
                {
                    return StatusCode(500, new { 
                        message = "Üye kayıt edildi ancak veritabanından okunamadı.", 
                        rowsAffected = rowsAffected 
                    });
                }

                // Şifreyi response'dan çıkar
                savedUye.Sifre = string.Empty;

                return CreatedAtAction(nameof(UyeKayit), new { id = savedUye.Id }, new
                {
                    id = savedUye.Id,
                    ad = savedUye.Ad,
                    soyad = savedUye.Soyad,
                    email = savedUye.Email,
                    telefon = savedUye.Telefon,
                    dogumTarihi = savedUye.DogumTarihi,
                    cinsiyet = savedUye.Cinsiyet,
                    adres = savedUye.Adres,
                    uyeNumarasi = savedUye.UyeNumarasi,
                    kayitTarihi = savedUye.KayitTarihi,
                    aktif = savedUye.Aktif,
                    message = "Üye başarıyla kaydedildi! (ID: " + savedUye.Id + ")"
                });
            }
            catch (DbUpdateException dbEx)
            {
                // ✅ TÜM NESTED INNER EXCEPTION'LARI YAKALA
                var allInnerExceptions = new List<string>();
                var currentEx = dbEx.InnerException;
                while (currentEx != null)
                {
                    allInnerExceptions.Add($"[{currentEx.GetType().Name}]: {currentEx.Message}");
                    currentEx = currentEx.InnerException;
                }
                
                // Ana inner exception bilgileri
                var innerExceptionMessage = dbEx.InnerException?.Message ?? "Inner exception yok";
                var innerExceptionStackTrace = dbEx.InnerException?.StackTrace ?? "Stack trace yok";
                var innerExceptionType = dbEx.InnerException?.GetType().FullName ?? "Bilinmiyor";
                
                // Logging - Database hatası
                _logger.LogError(dbEx, "Üye kaydı sırasında veritabanı hatası oluştu. Inner Exception: {InnerException}", innerExceptionMessage);
                
                // SQL Exception detayları
                object? sqlErrorDetails = null;
                if (dbEx.InnerException is SqlException sqlEx)
                {
                    _logger.LogError(sqlEx, "SQL Exception - Number: {Number}, Message: {Message}, State: {State}", 
                        sqlEx.Number, sqlEx.Message, sqlEx.State);
                    
                    sqlErrorDetails = new
                    {
                        number = sqlEx.Number,
                        message = sqlEx.Message,
                        severity = sqlEx.Class,
                        state = sqlEx.State,
                        procedure = sqlEx.Procedure ?? "",
                        lineNumber = sqlEx.LineNumber,
                        server = sqlEx.Server ?? "",
                        errors = sqlEx.Errors.Cast<SqlError>().Select(e => new
                        {
                            number = e.Number,
                            message = e.Message ?? "",
                            state = e.State,
                            severity = e.Class,
                            lineNumber = e.LineNumber,
                            source = e.Source ?? ""
                        }).ToList()
                    };
                }
                
                // Entity Framework entries detayları
                var entriesDetails = (dbEx.Entries?.Select(e => new
                {
                    entityType = e.Entity.GetType().Name,
                    state = e.State.ToString(),
                    properties = e.Properties.Select(p => new
                    {
                        name = p.Metadata.Name,
                        originalValue = p.OriginalValue?.ToString() ?? "null",
                        currentValue = p.CurrentValue?.ToString() ?? "null",
                        isModified = p.IsModified
                    }).ToList()
                }) ?? Enumerable.Empty<object>()).ToList();
                
                _logger.LogError("Entity Framework Entries: {Entries}", 
                    System.Text.Json.JsonSerializer.Serialize(entriesDetails));
                
                // ✅ MUTLAKA innerException gönder - null değil!
                return StatusCode(500, new
                {
                    message = "Üye kaydı sırasında veritabanı hatası oluştu.",
                    error = dbEx.Message ?? "Hata mesajı yok",
                    stackTrace = dbEx.StackTrace ?? "Stack trace yok",
                    innerException = innerExceptionMessage, // ✅ MUTLAKA VAR
                    innerExceptionType = innerExceptionType,
                    innerExceptionStackTrace = innerExceptionStackTrace, // ✅ MUTLAKA VAR
                    allInnerExceptions = allInnerExceptions, // ✅ Tüm nested exception'lar
                    sqlError = sqlErrorDetails, // SQL hatası varsa, yoksa null
                    entries = entriesDetails,
                    exceptionType = dbEx.GetType().FullName
                });
            }
            catch (Exception ex)
            {
                // ✅ TÜM NESTED INNER EXCEPTION'LARI YAKALA
                var allInnerExceptions = new List<string>();
                var currentEx = ex.InnerException;
                while (currentEx != null)
                {
                    allInnerExceptions.Add($"[{currentEx.GetType().Name}]: {currentEx.Message}");
                    currentEx = currentEx.InnerException;
                }
                
                // Ana inner exception bilgileri
                var innerExceptionMessage = ex.InnerException?.Message ?? "Inner exception yok";
                var innerExceptionStackTrace = ex.InnerException?.StackTrace ?? "Stack trace yok";
                var innerExceptionType = ex.InnerException?.GetType().FullName ?? "Bilinmiyor";
                
                // Logging - Genel hata
                _logger.LogError(ex, "Üye kaydı sırasında hata oluştu. Inner Exception: {InnerException}", innerExceptionMessage);
                
                // ✅ MUTLAKA innerException gönder - null değil!
                return StatusCode(500, new
                {
                    message = "Üye kaydı sırasında hata oluştu.",
                    error = ex.Message ?? "Hata mesajı yok",
                    stackTrace = ex.StackTrace ?? "Stack trace yok",
                    innerException = innerExceptionMessage, // ✅ MUTLAKA VAR
                    innerExceptionType = innerExceptionType,
                    innerExceptionStackTrace = innerExceptionStackTrace, // ✅ MUTLAKA VAR
                    allInnerExceptions = allInnerExceptions, // ✅ Tüm nested exception'lar
                    exceptionType = ex.GetType().FullName
                });
            }
        }

        /// <summary>
        /// Üye girişi yapar
        /// </summary>
        /// <param name="loginDto">Giriş bilgileri</param>
        /// <returns>Giriş sonucu ve üye bilgileri</returns>
        [HttpPost("giris")]
        public async Task<ActionResult<UyeGirisResponseDto>> UyeGiris([FromBody] UyeGirisDto loginDto)
        {
            try
            {
                // Giriş bilgileri kontrolü
                if (loginDto == null)
                {
                    _logger.LogWarning("Giriş isteği null geldi.");
                    return BadRequest(new { message = "Giriş bilgileri boş olamaz." });
                }

                if (string.IsNullOrWhiteSpace(loginDto.UyeNumarasi))
                {
                    return BadRequest(new { message = "Üye numarası boş olamaz." });
                }

                if (string.IsNullOrWhiteSpace(loginDto.Sifre))
                {
                    return BadRequest(new { message = "Şifre boş olamaz." });
                }

                // Veritabanı bağlantısını kontrol et
                try
                {
                    if (!await _context.Database.CanConnectAsync())
                    {
                        _logger.LogError("Veritabanı bağlantısı kurulamadı.");
                        return StatusCode(500, new { message = "Veritabanı bağlantı hatası. Lütfen daha sonra tekrar deneyin." });
                    }
                }
                catch (Exception dbConnEx)
                {
                    _logger.LogError(dbConnEx, "Veritabanı bağlantı kontrolü sırasında hata oluştu. Hata: {ErrorMessage}, InnerException: {InnerException}", 
                        dbConnEx.Message, dbConnEx.InnerException?.Message);
                    return StatusCode(500, new { 
                        message = "Veritabanı bağlantı hatası. Lütfen daha sonra tekrar deneyin.", 
                        error = dbConnEx.Message,
                        innerException = dbConnEx.InnerException?.Message
                    });
                }

                // Üyeyi veritabanından bul - sadece gerekli kolonları seç (paket kolonları yoksa hata vermesin)
                Uye? uye = null;
                try
                {
                    uye = await _context.Uyeler
                        .Where(u => u.UyeNumarasi == loginDto.UyeNumarasi && u.Aktif)
                        .Select(u => new Uye
                        {
                            Id = u.Id,
                            Ad = u.Ad,
                            Soyad = u.Soyad,
                            Email = u.Email,
                            Telefon = u.Telefon,
                            DogumTarihi = u.DogumTarihi,
                            Cinsiyet = u.Cinsiyet,
                            Adres = u.Adres,
                            UyeNumarasi = u.UyeNumarasi,
                            Sifre = u.Sifre,
                            KayitTarihi = u.KayitTarihi,
                            SonGirisTarihi = u.SonGirisTarihi,
                            Aktif = u.Aktif,
                            // Paket kolonları - nullable, yoksa null olur
                            UyelikTuru = u.UyelikTuru,
                            PaketBaslangicTarihi = u.PaketBaslangicTarihi,
                            PaketBitisTarihi = u.PaketBitisTarihi
                        })
                        .FirstOrDefaultAsync();
                }
                catch (Exception queryEx)
                {
                    // Paket kolonları yoksa, sadece temel kolonları çek
                    _logger.LogWarning(queryEx, "Paket kolonları bulunamadı, sadece temel kolonlar çekiliyor.");
                    uye = await _context.Uyeler
                        .Where(u => u.UyeNumarasi == loginDto.UyeNumarasi && u.Aktif)
                        .Select(u => new Uye
                        {
                            Id = u.Id,
                            Ad = u.Ad,
                            Soyad = u.Soyad,
                            Email = u.Email,
                            Telefon = u.Telefon,
                            DogumTarihi = u.DogumTarihi,
                            Cinsiyet = u.Cinsiyet,
                            Adres = u.Adres,
                            UyeNumarasi = u.UyeNumarasi,
                            Sifre = u.Sifre,
                            KayitTarihi = u.KayitTarihi,
                            SonGirisTarihi = u.SonGirisTarihi,
                            Aktif = u.Aktif
                        })
                        .FirstOrDefaultAsync();
                }

                if (uye == null)
                {
                    _logger.LogWarning("Geçersiz üye numarası ile giriş denemesi: {UyeNumarasi}", loginDto.UyeNumarasi);
                    return Unauthorized(new { message = "Geçersiz üye numarası veya şifre." });
                }

                // Şifre kontrolü
                if (!VerifyPassword(loginDto.Sifre, uye.Sifre))
                {
                    _logger.LogWarning("Geçersiz şifre ile giriş denemesi. Üye ID: {UyeId}", uye.Id);
                    return Unauthorized(new { message = "Geçersiz üye numarası veya şifre." });
                }

                // Son giriş tarihini güncelle
                try
                {
                    uye.SonGirisTarihi = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                catch (Exception saveEx)
                {
                    _logger.LogError(saveEx, "Son giriş tarihi güncellenirken hata oluştu. Üye ID: {UyeId}", uye.Id);
                    // Bu hata kritik değil, devam et
                }

                // JWT Token oluştur
                string token;
                try
                {
                    token = _jwtTokenService.GenerateToken(uye.Id.ToString(), "Uye", uye.Ad + " " + uye.Soyad);
                    
                    // Token'ın oluşturulduğunu doğrula
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogError("Token oluşturuldu ancak boş geldi! Üye ID: {UyeId}", uye.Id);
                        return StatusCode(500, new { 
                            message = "Token oluşturma hatası. Token boş geldi. Lütfen daha sonra tekrar deneyin."
                        });
                    }
                    
                    _logger.LogInformation("Token başarıyla oluşturuldu. Üye ID: {UyeId}, Token uzunluğu: {TokenLength}", 
                        uye.Id, token.Length);
                }
                catch (Exception tokenEx)
                {
                    _logger.LogError(tokenEx, "JWT token oluşturulurken hata oluştu. Üye ID: {UyeId}, Hata: {ErrorMessage}, InnerException: {InnerException}", 
                        uye.Id, tokenEx.Message, tokenEx.InnerException?.Message);
                    return StatusCode(500, new { 
                        message = "Token oluşturma hatası. Lütfen daha sonra tekrar deneyin.", 
                        error = tokenEx.Message,
                        innerException = tokenEx.InnerException?.Message,
                        stackTrace = tokenEx.StackTrace
                    });
                }

                // Response oluştur
                UyeGirisResponseDto response;
                try
                {
                    response = new UyeGirisResponseDto
                    {
                        Id = uye.Id,
                        Ad = uye.Ad ?? string.Empty,
                        Soyad = uye.Soyad ?? string.Empty,
                        Email = uye.Email ?? string.Empty,
                        Telefon = uye.Telefon ?? string.Empty,
                        DogumTarihi = uye.DogumTarihi,
                        Cinsiyet = uye.Cinsiyet ?? string.Empty,
                        Adres = uye.Adres ?? string.Empty,
                        UyeNumarasi = uye.UyeNumarasi ?? string.Empty,
                        KayitTarihi = uye.KayitTarihi,
                        SonGirisTarihi = uye.SonGirisTarihi,
                        Token = token,
                        Message = "Giriş başarılı"
                    };
                    
                    // Token'ın response'a eklendiğini doğrula
                    if (string.IsNullOrEmpty(response.Token))
                    {
                        _logger.LogError("Token response'a eklenemedi! Üye ID: {UyeId}, Token değişkeni: {TokenVariable}", 
                            uye.Id, token ?? "NULL");
                        return StatusCode(500, new { 
                            message = "Token oluşturuldu ancak response'a eklenemedi. Lütfen daha sonra tekrar deneyin.",
                            tokenCreated = !string.IsNullOrEmpty(token),
                            tokenLength = token?.Length ?? 0
                        });
                    }
                    
                    _logger.LogInformation("Token başarıyla oluşturuldu ve response'a eklendi. Üye ID: {UyeId}, Token uzunluğu: {TokenLength}, Response Token uzunluğu: {ResponseTokenLength}", 
                        uye.Id, token.Length, response.Token.Length);
                }
                catch (Exception responseEx)
                {
                    _logger.LogError(responseEx, "Response oluşturulurken hata oluştu. Üye ID: {UyeId}, Hata: {ErrorMessage}, InnerException: {InnerException}", 
                        uye.Id, responseEx.Message, responseEx.InnerException?.Message);
                    return StatusCode(500, new { 
                        message = "Response oluşturma hatası. Lütfen daha sonra tekrar deneyin.", 
                        error = responseEx.Message,
                        innerException = responseEx.InnerException?.Message,
                        stackTrace = responseEx.StackTrace
                    });
                }

                _logger.LogInformation("Başarılı giriş. Üye ID: {UyeId}, Üye Numarası: {UyeNumarasi}", uye.Id, uye.UyeNumarasi);
                return Ok(response);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Veritabanı hatası - Giriş işlemi sırasında hata oluştu.");
                return StatusCode(500, new { 
                    message = "Veritabanı hatası. Lütfen daha sonra tekrar deneyin.", 
                    error = dbEx.Message,
                    innerException = dbEx.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Giriş işlemi sırasında beklenmeyen hata oluştu. Hata: {ErrorMessage}, StackTrace: {StackTrace}", 
                    ex.Message, ex.StackTrace);
                return StatusCode(500, new { 
                    message = "Sunucu hatası! Lütfen daha sonra tekrar deneyin.", 
                    error = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Backend sağlık kontrolü
        /// </summary>
        /// <returns>Sağlık durumu</returns>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", message = "Uye API çalışıyor" });
        }

        /// <summary>
        /// Tüm üyeleri listeler
        /// </summary>
        /// <returns>Üye listesi</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Uye>>> GetUyeler()
        {
            try
            {
                // Önce paket kolonlarının var olup olmadığını kontrol et
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                
                bool hasPaketColumns = false;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_SCHEMA = 'dbo'
                        AND TABLE_NAME = 'Uyeler' 
                        AND COLUMN_NAME IN ('UyelikTuru', 'PaketBaslangicTarihi', 'PaketBitisTarihi')";
                    
                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    hasPaketColumns = count == 3; // Üç kolon da varsa true
                }
                
                await connection.CloseAsync();
                
                // Paket kolonları varsa tam bilgileri, yoksa sadece temel bilgileri getir
                if (hasPaketColumns)
                {
                    var uyeler = await _context.Uyeler
                        .Where(u => u.Aktif)
                        .Select(u => new Uye
                        {
                            Id = u.Id,
                            Ad = u.Ad,
                            Soyad = u.Soyad,
                            Email = u.Email,
                            Telefon = u.Telefon,
                            DogumTarihi = u.DogumTarihi,
                            Cinsiyet = u.Cinsiyet,
                            Adres = u.Adres,
                            UyeNumarasi = u.UyeNumarasi,
                            KayitTarihi = u.KayitTarihi,
                            SonGirisTarihi = u.SonGirisTarihi,
                            Aktif = u.Aktif,
                            UyelikTuru = u.UyelikTuru,
                            PaketBaslangicTarihi = u.PaketBaslangicTarihi,
                            PaketBitisTarihi = u.PaketBitisTarihi
                        })
                        .ToListAsync();

                    return Ok(uyeler);
                }
                else
                {
                    // Paket kolonları yoksa sadece temel bilgileri getir
                    var uyeler = await _context.Uyeler
                        .Where(u => u.Aktif)
                        .Select(u => new Uye
                        {
                            Id = u.Id,
                            Ad = u.Ad,
                            Soyad = u.Soyad,
                            Email = u.Email,
                            Telefon = u.Telefon,
                            DogumTarihi = u.DogumTarihi,
                            Cinsiyet = u.Cinsiyet,
                            Adres = u.Adres,
                            UyeNumarasi = u.UyeNumarasi,
                            KayitTarihi = u.KayitTarihi,
                            SonGirisTarihi = u.SonGirisTarihi,
                            Aktif = u.Aktif,
                            UyelikTuru = null,
                            PaketBaslangicTarihi = null,
                            PaketBitisTarihi = null
                        })
                        .ToListAsync();

                    return Ok(uyeler);
                }
            }
            catch (SqlException sqlEx) when (sqlEx.Message.Contains("Invalid column name"))
            {
                // Kolonlar yoksa sadece temel bilgileri getir
                _logger.LogWarning(sqlEx, "Paket kolonları bulunamadı, sadece temel bilgiler getiriliyor.");
                
                var uyeler = await _context.Uyeler
                    .Where(u => u.Aktif)
                    .Select(u => new Uye
                    {
                        Id = u.Id,
                        Ad = u.Ad,
                        Soyad = u.Soyad,
                        Email = u.Email,
                        Telefon = u.Telefon,
                        DogumTarihi = u.DogumTarihi,
                        Cinsiyet = u.Cinsiyet,
                        Adres = u.Adres,
                        UyeNumarasi = u.UyeNumarasi,
                        KayitTarihi = u.KayitTarihi,
                        SonGirisTarihi = u.SonGirisTarihi,
                        Aktif = u.Aktif,
                        UyelikTuru = null,
                        PaketBaslangicTarihi = null,
                        PaketBitisTarihi = null
                    })
                    .ToListAsync();

                return Ok(uyeler);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Üyeler listelenirken hata oluştu.");
                return StatusCode(500, new { message = "Üyeler listelenirken hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// ID'ye göre üye getirir
        /// </summary>
        /// <param name="id">Üye ID</param>
        /// <returns>Üye bilgileri</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Uye>> GetUye(int id)
        {
            try
            {
                // Önce paket kolonlarının var olup olmadığını kontrol et
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                
                bool hasPaketColumns = false;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_SCHEMA = 'dbo'
                        AND TABLE_NAME = 'Uyeler' 
                        AND COLUMN_NAME IN ('UyelikTuru', 'PaketBaslangicTarihi', 'PaketBitisTarihi')";
                    
                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    hasPaketColumns = count == 3; // Üç kolon da varsa true
                }
                
                await connection.CloseAsync();
                
                Uye? uye;
                
                // Paket kolonları varsa tam bilgileri, yoksa sadece temel bilgileri getir
                if (hasPaketColumns)
                {
                    uye = await _context.Uyeler
                        .Where(u => u.Id == id && u.Aktif)
                        .Select(u => new Uye
                        {
                            Id = u.Id,
                            Ad = u.Ad,
                            Soyad = u.Soyad,
                            Email = u.Email,
                            Telefon = u.Telefon,
                            DogumTarihi = u.DogumTarihi,
                            Cinsiyet = u.Cinsiyet,
                            Adres = u.Adres,
                            UyeNumarasi = u.UyeNumarasi,
                            KayitTarihi = u.KayitTarihi,
                            SonGirisTarihi = u.SonGirisTarihi,
                            Aktif = u.Aktif,
                            UyelikTuru = u.UyelikTuru,
                            PaketBaslangicTarihi = u.PaketBaslangicTarihi,
                            PaketBitisTarihi = u.PaketBitisTarihi
                        })
                        .FirstOrDefaultAsync();
                }
                else
                {
                    uye = await _context.Uyeler
                        .Where(u => u.Id == id && u.Aktif)
                        .Select(u => new Uye
                        {
                            Id = u.Id,
                            Ad = u.Ad,
                            Soyad = u.Soyad,
                            Email = u.Email,
                            Telefon = u.Telefon,
                            DogumTarihi = u.DogumTarihi,
                            Cinsiyet = u.Cinsiyet,
                            Adres = u.Adres,
                            UyeNumarasi = u.UyeNumarasi,
                            KayitTarihi = u.KayitTarihi,
                            SonGirisTarihi = u.SonGirisTarihi,
                            Aktif = u.Aktif,
                            UyelikTuru = null,
                            PaketBaslangicTarihi = null,
                            PaketBitisTarihi = null
                        })
                        .FirstOrDefaultAsync();
                }

                if (uye == null)
                {
                    return NotFound(new { message = "Üye bulunamadı." });
                }

                return Ok(uye);
            }
            catch (SqlException sqlEx) when (sqlEx.Message.Contains("Invalid column name"))
            {
                // Kolonlar yoksa sadece temel bilgileri getir
                _logger.LogWarning(sqlEx, "Paket kolonları bulunamadı, sadece temel bilgiler getiriliyor.");
                
                var uye = await _context.Uyeler
                    .Where(u => u.Id == id && u.Aktif)
                    .Select(u => new Uye
                    {
                        Id = u.Id,
                        Ad = u.Ad,
                        Soyad = u.Soyad,
                        Email = u.Email,
                        Telefon = u.Telefon,
                        DogumTarihi = u.DogumTarihi,
                        Cinsiyet = u.Cinsiyet,
                        Adres = u.Adres,
                        UyeNumarasi = u.UyeNumarasi,
                        KayitTarihi = u.KayitTarihi,
                        SonGirisTarihi = u.SonGirisTarihi,
                        Aktif = u.Aktif,
                        UyelikTuru = null,
                        PaketBaslangicTarihi = null,
                        PaketBitisTarihi = null
                    })
                    .FirstOrDefaultAsync();

                if (uye == null)
                {
                    return NotFound(new { message = "Üye bulunamadı." });
                }

                return Ok(uye);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Üye getirilirken hata oluştu.");
                return StatusCode(500, new { message = "Üye getirilirken hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Üye bilgilerini günceller
        /// </summary>
        /// <param name="id">Üye ID</param>
        /// <param name="uyeDto">Güncellenecek üye bilgileri</param>
        /// <returns>Güncellenmiş üye bilgileri</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<Uye>> UpdateUye(int id, [FromBody] UyeGuncellemeDto uyeDto)
        {
            try
            {
                var uye = await _context.Uyeler.FindAsync(id);

                if (uye == null)
                {
                    return NotFound(new { message = "Üye bulunamadı." });
                }

                // Email benzersizlik kontrolü (kendi email'i hariç)
                if (await _context.Uyeler.AnyAsync(u => u.Email == uyeDto.Email && u.Id != id))
                {
                    return BadRequest(new { message = "Bu email adresi zaten kullanılıyor." });
                }

                uye.Ad = uyeDto.Ad;
                uye.Soyad = uyeDto.Soyad;
                uye.Email = uyeDto.Email;
                uye.Telefon = uyeDto.Telefon;
                uye.DogumTarihi = uyeDto.DogumTarihi;
                uye.Cinsiyet = uyeDto.Cinsiyet;
                uye.Adres = uyeDto.Adres;

                await _context.SaveChangesAsync();

                // Şifreyi response'dan çıkar
                uye.Sifre = string.Empty;
                return Ok(new
                {
                    Id = uye.Id,
                    Ad = uye.Ad,
                    Soyad = uye.Soyad,
                    Email = uye.Email,
                    Telefon = uye.Telefon,
                    DogumTarihi = uye.DogumTarihi,
                    Cinsiyet = uye.Cinsiyet,
                    Adres = uye.Adres,
                    UyeNumarasi = uye.UyeNumarasi,
                    KayitTarihi = uye.KayitTarihi,
                    SonGirisTarihi = uye.SonGirisTarihi,
                    Aktif = uye.Aktif,
                    UyelikTuru = uye.UyelikTuru,
                    PaketBaslangicTarihi = uye.PaketBaslangicTarihi,
                    PaketBitisTarihi = uye.PaketBitisTarihi
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Üye güncelleme sırasında hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Üye paneli - sadece üyeler erişebilir
        /// </summary>
        /// <returns>Üye panel verileri</returns>
        [HttpGet("panel")]
        [Authorize]
        public async Task<ActionResult> UyePanel()
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

                // Üyeyi veritabanından bul
                var uye = await _context.Uyeler
                    .Where(u => u.Id == uyeId && u.Aktif)
                    .FirstOrDefaultAsync();

                if (uye == null)
                {
                    return NotFound(new { message = "Üye bulunamadı veya aktif değil." });
                }

                return Ok(new
                {
                    uye = new
                    {
                        Id = uye.Id,
                        Ad = uye.Ad,
                        Soyad = uye.Soyad,
                        Email = uye.Email,
                        Telefon = uye.Telefon,
                        DogumTarihi = uye.DogumTarihi,
                        Cinsiyet = uye.Cinsiyet,
                        Adres = uye.Adres,
                        UyeNumarasi = uye.UyeNumarasi,
                        KayitTarihi = uye.KayitTarihi,
                        SonGirisTarihi = uye.SonGirisTarihi,
                        UyelikTuru = uye.UyelikTuru,
                        PaketBaslangicTarihi = uye.PaketBaslangicTarihi,
                        PaketBitisTarihi = uye.PaketBitisTarihi
                    },
                    message = "Üye paneline hoş geldiniz!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Üye panel verileri alınırken hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Üyeyi siler (soft delete)
        /// </summary>
        /// <param name="id">Üye ID</param>
        /// <returns>Silme sonucu</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUye(int id)
        {
            try
            {
                var uye = await _context.Uyeler.FindAsync(id);

                if (uye == null)
                {
                    return NotFound(new { message = "Üye bulunamadı." });
                }

                uye.Aktif = false;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Üye başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Üye silme sırasında hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Giriş yapmış üyenin kendi üyeliğini sonlandırır ve çıkış yapar
        /// </summary>
        /// <returns>İşlem sonucu</returns>
        [HttpPost("uyelik-bitir")]
        [Authorize]
        public async Task<ActionResult> UyelikBitir()
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
                    return Forbid("Sadece üyeler bu işlemi yapabilir.");
                }

                // Kullanıcı ID'sini integer'a çevir
                if (!int.TryParse(userId, out int uyeId))
                {
                    return BadRequest(new { message = "Geçersiz kullanıcı ID formatı." });
                }

                // Üyeyi veritabanından bul
                var uye = await _context.Uyeler
                    .Where(u => u.Id == uyeId && u.Aktif)
                    .FirstOrDefaultAsync();

                if (uye == null)
                {
                    return NotFound(new { message = "Üye bulunamadı veya aktif değil." });
                }

                // Üyeliği sonlandır (soft delete)
                uye.Aktif = false;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Üyelik başarıyla sonlandırıldı. Çıkış yapabilirsiniz." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Üyelik sonlandırma sırasında hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Üye paket seçtikten sonra paket bilgilerini kaydeder
        /// </summary>
        /// <param name="paketDto">Paket bilgileri</param>
        /// <returns>Paket bilgileri</returns>
        [HttpPost("paket-baslat")]
        [Authorize]
        public async Task<ActionResult> PaketBaslat([FromBody] PaketBaslatDto paketDto)
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
                    return Forbid("Sadece üyeler bu işlemi yapabilir.");
                }

                // Kullanıcı ID'sini integer'a çevir
                if (!int.TryParse(userId, out int uyeId))
                {
                    return BadRequest(new { message = "Geçersiz kullanıcı ID formatı." });
                }

                // Üyeyi veritabanından bul
                var uye = await _context.Uyeler
                    .Where(u => u.Id == uyeId && u.Aktif)
                    .FirstOrDefaultAsync();

                if (uye == null)
                {
                    return NotFound(new { message = "Üye bulunamadı veya aktif değil." });
                }

                // Paket bilgilerini hesapla
                var paketBaslangicTarihi = DateTime.Now;
                var gunSayisi = paketDto.UyelikTuru switch
                {
                    1 => 1,   // Günlük
                    2 => 7,   // Haftalık
                    3 => 30,  // Aylık
                    4 => 365, // Yıllık
                    _ => 1
                };
                var paketBitisTarihi = paketBaslangicTarihi.AddDays(gunSayisi);

                // Önce paket kolonlarının var olduğundan emin ol
                try
                {
                    var connection = _context.Database.GetDbConnection();
                    await connection.OpenAsync();
                    
                    using (var command = connection.CreateCommand())
                    {
                        // Kolonları kontrol et ve yoksa ekle
                        command.CommandText = @"
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Uyeler]') AND name = 'UyelikTuru')
                            BEGIN
                                ALTER TABLE [Uyeler] ADD [UyelikTuru] INT NULL;
                            END
                            
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Uyeler]') AND name = 'PaketBaslangicTarihi')
                            BEGIN
                                ALTER TABLE [Uyeler] ADD [PaketBaslangicTarihi] DATETIME2 NULL;
                            END
                            
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Uyeler]') AND name = 'PaketBitisTarihi')
                            BEGIN
                                ALTER TABLE [Uyeler] ADD [PaketBitisTarihi] DATETIME2 NULL;
                            END";
                        
                        await command.ExecuteNonQueryAsync();
                    }
                    
                    await connection.CloseAsync();
                }
                catch (Exception colEx)
                {
                    _logger.LogWarning(colEx, "Paket kolonları kontrol edilirken hata oluştu, devam ediliyor...");
                    // Kolon ekleme hatası olsa bile devam et
                }

                // Üyeyi tekrar yükle (kolonlar eklendikten sonra)
                uye = await _context.Uyeler
                    .Where(u => u.Id == uyeId && u.Aktif)
                    .FirstOrDefaultAsync();

                if (uye == null)
                {
                    return NotFound(new { message = "Üye bulunamadı veya aktif değil." });
                }

                // Paket bilgilerini veritabanına kaydet
                uye.UyelikTuru = paketDto.UyelikTuru;
                uye.PaketBaslangicTarihi = paketBaslangicTarihi;
                uye.PaketBitisTarihi = paketBitisTarihi;
                
                // Sayaç PaketBaslangicTarihi'nden hesaplanır, veritabanında kolon yok

                // Değişiklikleri kaydet
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Paket kaydedildi - Üye ID: {uyeId}, Paket: {paketDto.UyelikTuru}, Başlangıç: {paketBaslangicTarihi}, Bitiş: {paketBitisTarihi}");

                return Ok(new
                {
                    paketBilgisi = new
                    {
                        uyelikTuru = paketDto.UyelikTuru,
                        paketBaslangicTarihi = paketBaslangicTarihi,
                        paketBitisTarihi = paketBitisTarihi
                    },
                    message = "Paket başarıyla başlatıldı."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Paket başlatma sırasında hata oluştu.", error = ex.Message });
            }
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
    }

    // DTO sınıfları
    public class UyeKayitDto
    {
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public DateTime DogumTarihi { get; set; }
        public string Cinsiyet { get; set; } = string.Empty;
        public string Adres { get; set; } = string.Empty;
        public string UyeNumarasi { get; set; } = string.Empty;
        public string Sifre { get; set; } = string.Empty;
    }

    public class UyeGirisDto
    {
        public string UyeNumarasi { get; set; } = string.Empty;
        public string Sifre { get; set; } = string.Empty;
    }

    public class UyeGirisResponseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("ad")]
        public string Ad { get; set; } = string.Empty;
        [JsonPropertyName("soyad")]
        public string Soyad { get; set; } = string.Empty;
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        [JsonPropertyName("telefon")]
        public string Telefon { get; set; } = string.Empty;
        [JsonPropertyName("dogumTarihi")]
        public DateTime DogumTarihi { get; set; }
        [JsonPropertyName("cinsiyet")]
        public string Cinsiyet { get; set; } = string.Empty;
        [JsonPropertyName("adres")]
        public string Adres { get; set; } = string.Empty;
        [JsonPropertyName("uyeNumarasi")]
        public string UyeNumarasi { get; set; } = string.Empty;
        [JsonPropertyName("kayitTarihi")]
        public DateTime KayitTarihi { get; set; }
        [JsonPropertyName("sonGirisTarihi")]
        public DateTime? SonGirisTarihi { get; set; }
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    public class UyeGuncellemeDto
    {
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public DateTime DogumTarihi { get; set; }
        public string Cinsiyet { get; set; } = string.Empty;
        public string Adres { get; set; } = string.Empty;
    }

    public class PaketBaslatDto
    {
        public int UyelikTuru { get; set; }
    }
}
