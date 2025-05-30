using Microsoft.EntityFrameworkCore;
using Nimi.Data.Models;

namespace Nimi.Data.DbContexts
{
    public class NimiDbContext : DbContext
    {
        private readonly string _dbPath;
        public NimiDbContext(string dbPath) => _dbPath = dbPath;

        public DbSet<Partner> Partners { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder ob)
            => ob.UseSqlite($"Data Source={_dbPath}");

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Partner>()
              .HasMany(p => p.Sales)
              .WithOne(s => s.Partner!)
              .HasForeignKey(s => s.PartnerId)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<Product>()
              .HasMany(p => p.Sales)
              .WithOne(s => s.Product!)
              .HasForeignKey(s => s.ProductId)
              .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
