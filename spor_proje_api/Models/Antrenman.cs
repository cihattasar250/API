using System.ComponentModel.DataAnnotations;

namespace spor_proje_api.Models
{
    public class Antrenman
    {
        public int Id { get; set; }
        
        public int? SporcuId { get; set; }
        
        public int? UyeId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string AntrenmanAdi { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Aciklama { get; set; }
        
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        public int Sure { get; set; } // Dakika cinsinden
        
        [StringLength(50)]
        public string? AntrenmanTipi { get; set; } // Kardiyo, Kuvvet, Esneklik vb.
        
        public int KaloriYakilan { get; set; }
        
        public int KalpAtisHizi { get; set; }
        
        [StringLength(200)]
        public string? Notlar { get; set; }
        
        // Navigation properties
        public virtual Sporcu? Sporcu { get; set; }
        public virtual Uye? Uye { get; set; }
    }
}
