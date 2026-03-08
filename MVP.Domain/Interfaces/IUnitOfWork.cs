using System.Threading.Tasks;

namespace MVP.Domain.Interfaces;

public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> CommitAsync();
    
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
