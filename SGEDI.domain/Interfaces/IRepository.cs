using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SGEDI.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null, 
            params Expression<Func<T, object>>[] includeProperties);
        
        Task<T?> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>> filter, 
            params Expression<Func<T, object>>[] includeProperties);
        
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
    }
}
