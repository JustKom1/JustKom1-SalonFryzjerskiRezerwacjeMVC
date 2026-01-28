using Microsoft.EntityFrameworkCore;

namespace Rezerwacje.Data
{
    public static class SeedData
    {
        public static async Task SeedServicesAsync(ApplicationDbContext context)
        {
            // Lista "wzorcowych" usług do seedowania / aktualizacji
            var seedServices = new List<Service>
            {
                new Service { Name = "Strzyżenie damskie", Description = "Mycie + strzyżenie + modelowanie", Price = 120, DurationMinutes = 90 },
                new Service { Name = "Strzyżenie męskie", Description = "Klasyczne lub fade", Price = 70, DurationMinutes = 30 },
                new Service { Name = "Koloryzacja", Description = "Farbowanie + pielęgnacja", Price = 220, DurationMinutes = 120 },
                new Service { Name = "Baleyage / Pasemka", Description = "Rozjaśnianie + tonowanie", Price = 300, DurationMinutes = 90 },
                new Service { Name = "Regeneracja włosów", Description = "Zabieg odbudowujący", Price = 150, DurationMinutes = 90 }
            };

            // Pobierz istniejące usługi z bazy
            var existing = await context.Services.ToListAsync();

            foreach (var seed in seedServices)
            {
                // szukamy po nazwie
                var dbItem = existing.FirstOrDefault(s => s.Name == seed.Name);

                if (dbItem == null)
                {
                    // Brak w bazie -> dodaj
                    context.Services.Add(seed);
                }
                else
                {
                    // Jest w bazie -> zaktualizuj pola (np. DurationMinutes było 0)
                    dbItem.Description = seed.Description;
                    dbItem.Price = seed.Price;
                    dbItem.DurationMinutes = seed.DurationMinutes;
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
