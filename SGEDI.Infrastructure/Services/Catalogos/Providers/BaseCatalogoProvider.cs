using Microsoft.EntityFrameworkCore;
using SGEDI.Application.DTOs;
using SGEDI.Application.Interfaces.Catalogos;
using SGEDI.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGEDI.Infrastructure.Services.Catalogos.Providers
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
            return entities.Select(x => new CatalogoItemDTO
            {
                Id = idSelector.Compile()(x),
                Descripcion = nombreSelector.Compile()(x)
            }).ToList();
        }
    }
}
