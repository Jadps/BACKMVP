using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using SGEDI.Domain.Interfaces;
using SGEDI.Infrastructure.Persistence;
using SGEDI.Infrastructure.Repositories;

namespace SGEDI.Infrastructure;

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

    public void Dispose()
    {
        _db.Dispose();
    }
}
