using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace spor_proje_api.Models
{
    public class Uye
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Ad { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Soyad { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Telefon { get; set; } = string.Empty;
        
        [Required]
        public DateTime DogumTarihi { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Cinsiyet { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Adres { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string UyeNumarasi { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string Sifre { get; set; } = string.Empty;
        
        public DateTime KayitTarihi { get; set; } = DateTime.Now;
        
        public bool Aktif { get; set; } = true;
        
        public DateTime? SonGirisTarihi { get; set; }
        
        // Paket bilgileri - nullable (paket seçilmediyse null)
        public int? UyelikTuru { get; set; } // 1: Günlük, 2: Haftalık, 3: Aylık, 4: Yıllık
        public DateTime? PaketBaslangicTarihi { get; set; }
        public DateTime? PaketBitisTarihi { get; set; }
        
        // Sayaç - PaketBaslangicTarihi'nden hesaplanır, veritabanında kolon yok (gizli)
        // Her seferinde PaketBaslangicTarihi'nden şu anki zamana kadar geçen süre hesaplanır
        [NotMapped]
        public long Sayac => PaketBaslangicTarihi.HasValue 
            ? (long)(DateTime.Now - PaketBaslangicTarihi.Value).TotalSeconds 
            : 0;
        
        // Navigation properties - Üye giriş yaptıktan sonra yaptığı işlemleri çekmek için
        public virtual ICollection<Antrenman> Antrenmanlar { get; set; } = new List<Antrenman>();
        public virtual ICollection<Performans> Performanslar { get; set; } = new List<Performans>();
        public virtual ICollection<Beslenme> BeslenmeKayitlari { get; set; } = new List<Beslenme>();
        public virtual ICollection<Hedef> Hedefler { get; set; } = new List<Hedef>();
    }
}
