using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MVP.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null, 
            bool disableTracking = true,
            params Expression<Func<T, object>>[] includeProperties);

        Task<(List<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            bool disableTracking = true,
            params Expression<Func<T, object>>[] includeProperties);
        
        Task<T?> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>> filter, 
            bool disableTracking = true,
            params Expression<Func<T, object>>[] includeProperties);
        
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
    }
}
