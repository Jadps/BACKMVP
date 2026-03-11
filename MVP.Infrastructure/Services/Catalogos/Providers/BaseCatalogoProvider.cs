using Microsoft.EntityFrameworkCore;
using MVP.Application.DTOs;
using MVP.Application.Interfaces.Catalogos;
using MVP.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVP.Infrastructure.Services.Catalogos.Providers
{
    public abstract class BaseCatalogoProvider : ICatalogoProvider
    {
        protected readonly ApplicationDbContext _db;

        protected BaseCatalogoProvider(ApplicationDbContext db)
        {
            _db = db;
        }

        public abstract string Nombre { get; }

        public abstract Task<List<CatalogoItemDTO>> ObtenerItemsAsync();

        protected async Task<List<CatalogoItemDTO>> ProyectarCatalogo<T>(
            IQueryable<T> query,
            System.Linq.Expressions.Expression<System.Func<T, Guid>> idSelector,
            System.Linq.Expressions.Expression<System.Func<T, string>> nombreSelector)
        {
            var entities = await query.ToListAsync();
            return entities.Select(x => new CatalogoItemDTO(
                idSelector.Compile()(x),
                nombreSelector.Compile()(x)
            )).ToList();
        }
    }
}
