using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;


namespace LinkUpPro.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T>
            where T : class{

        protected readonly LinkUpAppContext _context;

        public GenericRepository(LinkUpAppContext context)
        {
            _context = context;
        }

        public virtual async Task<T?> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<List<T>?> AddRangeAsync(List<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            return entities;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task<List<T>> GetAllListAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public virtual async Task<List<T>> GetAllListWithInclude(List<string> properties)
        {
            var query = _context.Set<T>().AsQueryable();
            foreach (var property in properties)
            {
                query = query.Include(property);
            }
            return await query.ToListAsync();
        }

        public virtual IQueryable<T> GetAllQuery()
        {
            return _context.Set<T>().AsQueryable();
        }

        public virtual IQueryable<T> GetAllQueryWithInclude(List<string> properties)
        {
            var query = _context.Set<T>().AsQueryable();
            foreach (var property in properties)
            {
                query = query.Include(property);
            }
            return query;
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public virtual async Task<T?> UpdateAsync(int id, T entity)
        {
            var existingEntity = await _context.Set<T>().FindAsync(id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
                return existingEntity;
            }

            return null;

        }
    }
}
