using System.ComponentModel.DataAnnotations;

namespace spor_proje_api.Models
{
    public class UcretGuncelleme
    {
        public int Id { get; set; }
        
        [Required]
        public int UyeId { get; set; }
        
        [Required]
        public decimal EskiUcret { get; set; }
        
        [Required]
        public decimal YeniUcret { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ArtisNedeni { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Aciklama { get; set; }
        
        [Required]
        public DateTime GecerlilikTarihi { get; set; }
        
        [Required]
        public DateTime GuncellemeTarihi { get; set; } = DateTime.Now;
        
        [Required]
        public int GuncelleyenAdminId { get; set; }
        
        public bool Aktif { get; set; } = true;
        
        // Navigation properties
        public virtual Uye Uye { get; set; } = null!;
        public virtual Admin GuncelleyenAdmin { get; set; } = null!;
    }
}
