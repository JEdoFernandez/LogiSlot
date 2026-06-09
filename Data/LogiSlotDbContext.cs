using Microsoft.EntityFrameworkCore;
using LogiSlot.Models;

namespace LogiSlot.Data
{
    // Esta clase controla de Entity Framework Core (nuestro ORM).
    // Se encarga de conectar nuestras clases (Modelos) con las tablas de la base de datos PostgreSQL.
    public class LogiSlotDbContext : DbContext
    {
        public LogiSlotDbContext(DbContextOptions<LogiSlotDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Almacen> Almacenes { get; set; } = null!;
        public DbSet<Cita> Citas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configura relaciones
            modelBuilder.Entity<Cita>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(c => c.TransportistaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cita>()
                .HasOne<Almacen>()
                .WithMany()
                .HasForeignKey(c => c.AlmacenId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
