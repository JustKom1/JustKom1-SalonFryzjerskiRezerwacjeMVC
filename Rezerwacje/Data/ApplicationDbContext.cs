using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rezerwacje.Models;

namespace Rezerwacje.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Service> Services { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<SalonDayOverride> SalonDayOverrides { get; set; }
    }
}
