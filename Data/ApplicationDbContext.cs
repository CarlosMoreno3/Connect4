using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Connect4.Models;

namespace Connect4.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Jugador> Jugador { get; set; }

        public DbSet<Movimiento> Movimientos { get; set; }

        public DbSet<Partida> Partidas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Partida>()
                .HasOne(p => p.Jugador1)
                .WithMany()
                .HasForeignKey(p => p.Jugador1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Partida>()
                .HasOne(p => p.Jugador2)
                .WithMany()
                .HasForeignKey(p => p.Jugador2Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Movimiento>()
                .Property(m => m.Columna)
                .HasConversion<string>();
        }
    }
}