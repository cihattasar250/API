using Microsoft.EntityFrameworkCore;
using spor_proje_api.Models;

namespace spor_proje_api.Data
{
    public class SporDbContext : DbContext
    {
        public SporDbContext(DbContextOptions<SporDbContext> options) : base(options)
        {
        }

        public DbSet<Sporcu> Sporcular { get; set; }
        public DbSet<Antrenman> Antrenmanlar { get; set; }
        public DbSet<Performans> Performanslar { get; set; }
        public DbSet<Beslenme> BeslenmeKayitlari { get; set; }
        public DbSet<Hedef> Hedefler { get; set; }
        public DbSet<Admin> Adminler { get; set; }
        public DbSet<Uye> Uyeler { get; set; }
        public DbSet<PerformansTakibi> PerformansTakibi { get; set; }
        public DbSet<UcretGuncelleme> UcretGuncellemeleri { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Sporcu konfigürasyonu
            modelBuilder.Entity<Sporcu>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Ad).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Soyad).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SporDali).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.Telefon).HasMaxLength(20);
                entity.Property(e => e.KayitTarihi).HasDefaultValueSql("GETDATE()");
            });

            // Antrenman konfigürasyonu
            modelBuilder.Entity<Antrenman>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AntrenmanAdi).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Aciklama).HasMaxLength(500);
                entity.Property(e => e.AntrenmanTipi).HasMaxLength(50);
                entity.Property(e => e.Notlar).HasMaxLength(200);
                entity.Property(e => e.Tarih).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Sporcu)
                      .WithMany(s => s.Antrenmanlar)
                      .HasForeignKey(e => e.SporcuId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Uye)
                      .WithMany(u => u.Antrenmanlar)
                      .HasForeignKey(e => e.UyeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Performans konfigürasyonu
            modelBuilder.Entity<Performans>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PerformansAdi).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Birim).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Aciklama).HasMaxLength(200);
                entity.Property(e => e.Kategori).HasMaxLength(100);
                entity.Property(e => e.Tarih).HasDefaultValueSql("GETDATE()");
                
                // Decimal precision
                entity.Property(e => e.Deger).HasPrecision(18, 2);

                entity.HasOne(e => e.Sporcu)
                      .WithMany(s => s.Performanslar)
                      .HasForeignKey(e => e.SporcuId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Uye)
                      .WithMany(u => u.Performanslar)
                      .HasForeignKey(e => e.UyeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Beslenme konfigürasyonu
            modelBuilder.Entity<Beslenme>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.YemekAdi).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Ogun).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Aciklama).HasMaxLength(200);
                entity.Property(e => e.Tarih).HasDefaultValueSql("GETDATE()");
                
                // Decimal precision
                entity.Property(e => e.Kalori).HasPrecision(18, 2);
                entity.Property(e => e.Protein).HasPrecision(18, 2);
                entity.Property(e => e.Karbonhidrat).HasPrecision(18, 2);
                entity.Property(e => e.Yag).HasPrecision(18, 2);

                entity.HasOne(e => e.Sporcu)
                      .WithMany(s => s.BeslenmeKayitlari)
                      .HasForeignKey(e => e.SporcuId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Uye)
                      .WithMany(u => u.BeslenmeKayitlari)
                      .HasForeignKey(e => e.UyeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Hedef konfigürasyonu
            modelBuilder.Entity<Hedef>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.HedefAdi).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Aciklama).HasMaxLength(500);
                entity.Property(e => e.Kategori).HasMaxLength(100);
                entity.Property(e => e.Birim).HasMaxLength(50);
                entity.Property(e => e.BaslangicTarihi).HasDefaultValueSql("GETDATE()");
                
                // Decimal precision
                entity.Property(e => e.HedefDeger).HasPrecision(18, 2);

                entity.HasOne(e => e.Sporcu)
                      .WithMany(s => s.Hedefler)
                      .HasForeignKey(e => e.SporcuId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Uye)
                      .WithMany(u => u.Hedefler)
                      .HasForeignKey(e => e.UyeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Admin konfigürasyonu
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Ad).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Soyad).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Telefon).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AdminNumarasi).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Sifre).IsRequired().HasMaxLength(255);
                entity.Property(e => e.KayitTarihi).HasDefaultValueSql("GETDATE()");
                
                // Admin numarası benzersiz olmalı
                entity.HasIndex(e => e.AdminNumarasi).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Üye konfigürasyonu
            modelBuilder.Entity<Uye>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Ad).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Soyad).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Telefon).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Cinsiyet).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Adres).IsRequired().HasMaxLength(500);
                entity.Property(e => e.UyeNumarasi).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Sifre).IsRequired().HasMaxLength(255);
                entity.Property(e => e.KayitTarihi).HasDefaultValueSql("GETDATE()");
                
                // Paket bilgileri - nullable
                entity.Property(e => e.UyelikTuru).IsRequired(false);
                entity.Property(e => e.PaketBaslangicTarihi).IsRequired(false);
                entity.Property(e => e.PaketBitisTarihi).IsRequired(false);
                
                // Üye numarası benzersiz olmalı
                entity.HasIndex(e => e.UyeNumarasi).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // PerformansTakibi konfigürasyonu
            modelBuilder.Entity<PerformansTakibi>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Tarih).IsRequired();
                entity.Property(e => e.Kilo).IsRequired().HasPrecision(18, 2);
                entity.Property(e => e.VucutYagOrani).HasPrecision(18, 2);
                entity.Property(e => e.KasKutlesi).HasPrecision(18, 2);
                entity.Property(e => e.Notlar).HasMaxLength(1000);
                entity.Property(e => e.KayitTarihi).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Uye)
                      .WithMany()
                      .HasForeignKey(e => e.UyeId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint kaldırıldı - kullanıcı istediği kadar performans kaydı ekleyebilir
            });

            // UcretGuncelleme konfigürasyonu
            modelBuilder.Entity<UcretGuncelleme>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UyeId).IsRequired();
                entity.Property(e => e.EskiUcret).IsRequired().HasPrecision(18, 2);
                entity.Property(e => e.YeniUcret).IsRequired().HasPrecision(18, 2);
                entity.Property(e => e.ArtisNedeni).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Aciklama).HasMaxLength(1000);
                entity.Property(e => e.GecerlilikTarihi).IsRequired();
                entity.Property(e => e.GuncellemeTarihi).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.GuncelleyenAdminId).IsRequired();

                entity.HasOne(e => e.Uye)
                      .WithMany()
                      .HasForeignKey(e => e.UyeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.GuncelleyenAdmin)
                      .WithMany()
                      .HasForeignKey(e => e.GuncelleyenAdminId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
