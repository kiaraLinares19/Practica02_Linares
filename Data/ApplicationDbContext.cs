using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Practica2.Models; 

namespace Practica2.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Inmueble> Inmuebles { get; set; }
        public DbSet<Visita> Visitas { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        // -------------------------

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Inmueble>()
                .HasIndex(i => i.Codigo)
                .IsUnique();

            modelBuilder.Entity<Visita>()
                .HasOne(v => v.Inmueble)
                .WithMany()
                .HasForeignKey(v => v.InmuebleId)
                .OnDelete(DeleteBehavior.Restrict); 

        
            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Inmueble)
                .WithMany()
                .HasForeignKey(r => r.InmuebleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}