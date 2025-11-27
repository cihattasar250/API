using System.ComponentModel.DataAnnotations;

namespace spor_proje_api.Models
{
    public class Sporcu
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Ad { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Soyad { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string SporDali { get; set; } = string.Empty;
        
        public int Yas { get; set; }
        
        [StringLength(200)]
        public string? Email { get; set; }
        
        [StringLength(20)]
        public string? Telefon { get; set; }
        
        public DateTime KayitTarihi { get; set; } = DateTime.Now;
        
        public bool Aktif { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<Antrenman> Antrenmanlar { get; set; } = new List<Antrenman>();
        public virtual ICollection<Performans> Performanslar { get; set; } = new List<Performans>();
        public virtual ICollection<Beslenme> BeslenmeKayitlari { get; set; } = new List<Beslenme>();
        public virtual ICollection<Hedef> Hedefler { get; set; } = new List<Hedef>();
    }
}
