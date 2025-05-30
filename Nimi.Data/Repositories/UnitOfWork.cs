using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Nimi.Data.DbContexts;
using Nimi.Data.Models;


namespace Nimi.Data.Repositories
{
    public class UnitOfWork
    {
        private readonly NimiDbContext _ctx;
        public INimiRepository<Partner> Partners { get; }
        public INimiRepository<Product> Products { get; }
        public INimiRepository<Sale> Sales { get; }

        public UnitOfWork(string dbPath)
        {
            _ctx = new NimiDbContext(dbPath);
            _ctx.Database.EnsureCreated();
            Partners = new Repository<Partner>(_ctx);
            Products = new Repository<Product>(_ctx);
            Sales = new Repository<Sale>(_ctx);
        }

        public void Save() => _ctx.SaveChanges();

        public EntityEntry Entry(object entity)
        {
            return _ctx.Entry(entity);
        }

    }
}
