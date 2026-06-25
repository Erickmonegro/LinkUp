using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<List<T>> GetAllListAsync();
        Task<T?> AddAsync(T entity);
        IQueryable<T> GetAllQuery();
        Task<T?> UpdateAsync(int id, T entity);
        Task DeleteAsync(int id);
        Task<List<T>?> AddRangeAsync(List<T> entities);
        Task<List<T>> GetAllListWithInclude(List<string> properties);
        IQueryable<T> GetAllQueryWithInclude(List<string> properties);
    }
}
