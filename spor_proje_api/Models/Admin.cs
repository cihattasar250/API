using System.ComponentModel.DataAnnotations;

namespace spor_proje_api.Models
{
    public class Admin
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
        [StringLength(50)]
        public string AdminNumarasi { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string Sifre { get; set; } = string.Empty;
        
        public DateTime KayitTarihi { get; set; } = DateTime.Now;
        
        public bool Aktif { get; set; } = true;
        
        public DateTime? SonGirisTarihi { get; set; }
    }
}
