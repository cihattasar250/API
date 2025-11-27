using System.ComponentModel.DataAnnotations;

namespace spor_proje_api.Models
{
    public class Beslenme
    {
        public int Id { get; set; }
        
        public int? SporcuId { get; set; }
        
        public int? UyeId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string YemekAdi { get; set; } = string.Empty;
        
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        [StringLength(50)]
        public string Ogun { get; set; } = string.Empty; // Kahvaltı, Öğle, Akşam, Atıştırmalık
        
        public decimal Kalori { get; set; }
        
        public decimal Protein { get; set; } // gram
        public decimal Karbonhidrat { get; set; } // gram
        public decimal Yag { get; set; } // gram
        
        [StringLength(200)]
        public string? Aciklama { get; set; }
        
        // Navigation properties
        public virtual Sporcu? Sporcu { get; set; }
        public virtual Uye? Uye { get; set; }
    }
}
