using System.ComponentModel.DataAnnotations;

namespace spor_proje_api.Models
{
    public class Performans
    {
        public int Id { get; set; }
        
        public int? SporcuId { get; set; }
        
        public int? UyeId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string PerformansAdi { get; set; } = string.Empty;
        
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        public decimal Deger { get; set; }
        
        [StringLength(50)]
        public string Birim { get; set; } = string.Empty; // kg, metre, saniye vb.
        
        [StringLength(200)]
        public string? Aciklama { get; set; }
        
        [StringLength(100)]
        public string? Kategori { get; set; } // Kuvvet, Hız, Dayanıklılık vb.
        
        // Navigation properties
        public virtual Sporcu? Sporcu { get; set; }
        public virtual Uye? Uye { get; set; }
    }
}
