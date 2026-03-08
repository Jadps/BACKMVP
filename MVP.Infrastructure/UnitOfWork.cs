using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MVP.Domain.Interfaces;
using MVP.Infrastructure.Persistence;
using MVP.Infrastructure.Repositories;

namespace MVP.Infrastructure;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly ConcurrentDictionary<string, object> _repositories = new();

    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
    }

    public IRepository<T> Repository<T>() where T : class
    {
        string typeName = typeof(T).Name;

        return (IRepository<T>)_repositories.GetOrAdd(typeName, _ => new Repository<T>(_db));
    }

    public async Task<int> CommitAsync()
    {
        return await _db.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        await _db.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _db.Database.CommitTransactionAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await _db.Database.RollbackTransactionAsync();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
