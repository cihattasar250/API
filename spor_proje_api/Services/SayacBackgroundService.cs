using Microsoft.EntityFrameworkCore;
using spor_proje_api.Data;

namespace spor_proje_api.Services
{
    public class SayacBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SayacBackgroundService> _logger;
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1); // Her saniye güncelle

        public SayacBackgroundService(IServiceProvider serviceProvider, ILogger<SayacBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // İlk başlatmada biraz bekle (veritabanı hazır olsun)
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            
            _logger.LogInformation("Sayaç Background Service başlatıldı.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Her saniye aktif üyelerin sayaçlarını artır
                    await UpdateSayaclar(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Sayaç güncelleme sırasında hata oluştu: {Message}", ex.Message);
                    // Hata durumunda 5 saniye bekle
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }

                // 1 saniye bekle
                await Task.Delay(_updateInterval, stoppingToken);
            }

            _logger.LogInformation("Sayaç Background Service durduruldu.");
        }

        private async Task UpdateSayaclar(CancellationToken cancellationToken)
        {
            // Sayac kolonu yok - PaketBaslangicTarihi'nden hesaplanıyor
            // Bu service artık gerekli değil çünkü sayaç her seferinde hesaplanıyor
            // Service'i devre dışı bırakabiliriz ama şimdilik boş bırakıyoruz
            await Task.CompletedTask;
        }
    }
}

