using spor_proje_api.Data;

namespace spor_proje_api.Seeders
{
    public static class DataSeeder
    {
        public static Task SeedAsync(SporDbContext context)
        {
            // Artık test verisi eklenmiyor. Var olan kayıtlar korunur.
            return Task.CompletedTask;
        }
    }
}
