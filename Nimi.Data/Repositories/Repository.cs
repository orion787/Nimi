using Microsoft.EntityFrameworkCore;
using Nimi.Core.Entities;
using Nimi.Data.DbContexts;

namespace Nimi.Data.Repositories
{
    public class Repository<T> : INimiRepository<T> where T : EntityBase
    {
        private readonly NimiDbContext _ctx;
        private readonly DbSet<T> _dbSet;

        public Repository(NimiDbContext ctx)
        {
            _ctx = ctx;
            _dbSet = _ctx.Set<T>();
        }

        public IEnumerable<T> GetAll()
            => _dbSet.AsNoTracking().ToList();

        public T? GetById(int id)
            => _dbSet.Find(id);

        public void Add(T entity)
            => _dbSet.Add(entity);

        public void Update(T entity)
            => _dbSet.Update(entity);

        public void Remove(T entity)
            => _dbSet.Remove(entity);
    }
}
