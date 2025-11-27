using System.ComponentModel.DataAnnotations;

namespace spor_proje_api.Models
{
    public class Hedef
    {
        public int Id { get; set; }
        
        public int? SporcuId { get; set; }
        
        public int? UyeId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string HedefAdi { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Aciklama { get; set; }
        
        public DateTime BaslangicTarihi { get; set; } = DateTime.Now;
        
        public DateTime HedefTarihi { get; set; }
        
        public bool Tamamlandi { get; set; } = false;
        
        public DateTime? TamamlanmaTarihi { get; set; }
        
        [StringLength(100)]
        public string? Kategori { get; set; } // Kilo, Performans, Antrenman vb.
        
        public decimal? HedefDeger { get; set; }
        
        [StringLength(50)]
        public string? Birim { get; set; }
        
        // Navigation properties
        public virtual Sporcu? Sporcu { get; set; }
        public virtual Uye? Uye { get; set; }
    }
}
