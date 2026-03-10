using Microsoft.EntityFrameworkCore;
using MVP.Domain.Interfaces;
using MVP.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MVP.Infrastructure.Repositories
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
            bool disableTracking = true,
            params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = DbSet;
            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

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

        public async Task<(List<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            bool disableTracking = true,
            params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = DbSet;
            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            int totalCount = await query.CountAsync();

            foreach (var includeProp in includeProperties)
            {
                query = query.Include(includeProp);
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<T?> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>> filter, 
            bool disableTracking = true,
            params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = DbSet;
            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

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
