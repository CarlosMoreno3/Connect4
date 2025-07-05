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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}