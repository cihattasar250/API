using Microsoft.EntityFrameworkCore;
using spor_proje_api.Data;
using spor_proje_api.Seeders;
using spor_proje_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // JSON property name policy - hem camelCase hem PascalCase kabul et
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // PascalCase
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // Case insensitive
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Spor Proje API",
        Version = "v1",
        Description = "Sporcu takip sistemi için RESTful API",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Spor Proje API",
            Email = "info@sporproje.com"
        }
    });
    
    // XML dokümantasyon dosyasını dahil et
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Entity Framework DbContext ekleme
builder.Services.AddDbContext<SporDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication ekleme
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // Token süresi kontrolünü kapat
            ValidateIssuerSigningKey = true,
            ValidIssuer = "SporProjeAPI",
            ValidAudience = "SporProjeUsers",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("BuCokGizliBirAnahtar123456789012345678901234567890"))
        };
    });

builder.Services.AddAuthorization();

// JWT Token Service ekleme
builder.Services.AddScoped<JwtTokenService>();

// Background Service - Sayaç otomatik güncelleme (DEVRE DIŞI - Sayac kolonu yok, PaketBaslangicTarihi'nden hesaplanıyor)
// builder.Services.AddHostedService<SayacBackgroundService>();

// CORS politikası ekleme (HTML projesi için)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Swagger her ortamda aktif
app.UseSwagger();
app.UseSwaggerUI();

// HTTPS redirection sadece Development ortamında kullanılmalı
// Render gibi production platformlarında reverse proxy HTTPS'i handle eder
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// CORS middleware'i ekleme
app.UseCors("AllowAll");

// Authentication ve Authorization middleware'leri
app.UseAuthentication();
app.UseAuthorization();

// Root path'i Swagger'a yönlendir
app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

app.MapControllers();

// Veritabanı resetleme DEVRE DIŞI: Artık uygulama başlarken veriler silinmeyecek.
// Gerekirse manuel olarak 'reset_database.sql' scriptini çalıştırabilirsin.

// Paket kolonlarını otomatik ekle ve Sayac kolonunu kaldır
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SporDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Veritabanı bağlantısını kontrol et
        if (await context.Database.CanConnectAsync())
        {
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            
            // 1. Sayac kolonunu kaldır (eğer varsa)
            command.CommandText = @"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Uyeler]') AND name = 'Sayac')
                BEGIN
                    ALTER TABLE [Uyeler] DROP COLUMN [Sayac];
                    SELECT 'Sayac kolonu kaldırıldı' AS Result;
                END
                ELSE
                BEGIN
                    SELECT 'Sayac kolonu zaten yok' AS Result;
                END";
            
            var sayacResult = await command.ExecuteScalarAsync();
            logger.LogInformation("Sayac kolonu kontrolü: {Result}", sayacResult);
            
            // 2. Paket kolonlarını ekle (eğer yoksa)
            command.CommandText = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Uyeler]') AND name = 'UyelikTuru')
                BEGIN
                    ALTER TABLE [Uyeler] ADD [UyelikTuru] INT NULL;
                    SELECT 'UyelikTuru kolonu eklendi' AS Result;
                END
                ELSE
                BEGIN
                    SELECT 'UyelikTuru kolonu zaten mevcut' AS Result;
                END";
            
            var uyelikTuruResult = await command.ExecuteScalarAsync();
            logger.LogInformation("UyelikTuru kolonu kontrolü: {Result}", uyelikTuruResult);
            
            command.CommandText = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Uyeler]') AND name = 'PaketBaslangicTarihi')
                BEGIN
                    ALTER TABLE [Uyeler] ADD [PaketBaslangicTarihi] DATETIME2 NULL;
                    SELECT 'PaketBaslangicTarihi kolonu eklendi' AS Result;
                END
                ELSE
                BEGIN
                    SELECT 'PaketBaslangicTarihi kolonu zaten mevcut' AS Result;
                END";
            
            var paketBaslangicResult = await command.ExecuteScalarAsync();
            logger.LogInformation("PaketBaslangicTarihi kolonu kontrolü: {Result}", paketBaslangicResult);
            
            command.CommandText = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Uyeler]') AND name = 'PaketBitisTarihi')
                BEGIN
                    ALTER TABLE [Uyeler] ADD [PaketBitisTarihi] DATETIME2 NULL;
                    SELECT 'PaketBitisTarihi kolonu eklendi' AS Result;
                END
                ELSE
                BEGIN
                    SELECT 'PaketBitisTarihi kolonu zaten mevcut' AS Result;
                END";
            
            var paketBitisResult = await command.ExecuteScalarAsync();
            logger.LogInformation("PaketBitisTarihi kolonu kontrolü: {Result}", paketBitisResult);
            
            await connection.CloseAsync();
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Veritabanı kolon kontrolü sırasında hata oluştu (kritik değil): {Message}", ex.Message);
        // Hata olsa bile uygulama çalışmaya devam etsin
    }
}

app.Run();
