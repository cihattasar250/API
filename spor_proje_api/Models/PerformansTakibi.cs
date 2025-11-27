using System.ComponentModel.DataAnnotations;

namespace spor_proje_api.Models
{
    public class PerformansTakibi
    {
        public int Id { get; set; }
        
        [Required]
        public int UyeId { get; set; }
        
        [Required]
        public DateTime Tarih { get; set; }
        
        [Required]
        [Range(0.1, 500.0, ErrorMessage = "Kilo 0.1 ile 500 kg arasında olmalıdır")]
        public decimal Kilo { get; set; }
        
        [Range(0.0, 100.0, ErrorMessage = "Vücut yağ oranı 0 ile 100% arasında olmalıdır")]
        public decimal? VucutYagOrani { get; set; }
        
        [Range(0.1, 200.0, ErrorMessage = "Kas kütlesi 0.1 ile 200 kg arasında olmalıdır")]
        public decimal? KasKutlesi { get; set; }
        
        [Range(0, 1440, ErrorMessage = "Kardiyo süresi 0 ile 1440 dakika arasında olmalıdır")]
        public int? KardiyoSuresi { get; set; }
        
        [StringLength(1000)]
        public string? Notlar { get; set; }
        
        public DateTime KayitTarihi { get; set; } = DateTime.Now;
        
        // Navigation property
        public virtual Uye? Uye { get; set; }
    }
}
