using System.ComponentModel.DataAnnotations;

namespace spor_proje_api.Models
{
    public class UcretGuncellemeRequest
    {
        [Required]
        public int UyeId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ücret 0'dan büyük olmalıdır")]
        public decimal YeniUcret { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ArtisNedeni { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Aciklama { get; set; }
        
        [Required]
        public DateTime GecerlilikTarihi { get; set; }
    }
}
