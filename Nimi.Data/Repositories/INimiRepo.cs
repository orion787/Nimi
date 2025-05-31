using Nimi.Core.Entities;

namespace Nimi.Data.Repositories
{
    public interface INimiRepository<T> where T : EntityBase
    {
        IEnumerable<T> GetAll();
        T? GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void Remove(T entity);
        public IQueryable<T> Query();
    }
}
