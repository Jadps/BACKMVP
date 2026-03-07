using Microsoft.EntityFrameworkCore;
using SGEDI.Domain.Interfaces;
using SGEDI.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SGEDI.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbSet<T> DbSet;

        public Repository(ApplicationDbContext db)
        {
            this.DbSet = db.Set<T>();
        }

        public async Task<List<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null, 
            params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = DbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProp in includeProperties)
            {
                query = query.Include(includeProp);
            }

            return await query.ToListAsync();
        }

        public async Task<T?> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>> filter, 
            params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = DbSet;
            query = query.Where(filter);
            
            foreach (var includeProp in includeProperties)
            {
                query = query.Include(includeProp);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task AddAsync(T entity)
        {
            await DbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            DbSet.Update(entity);
        }

        public void Remove(T entity)
        {
            if (entity is ISoftDelete softDeleteEntity)
            {
                softDeleteEntity.Borrado = true;
                DbSet.Update(entity);
            }
            else
            {
                DbSet.Remove(entity);
            }
        }

    }
}
