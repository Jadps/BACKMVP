using Microsoft.EntityFrameworkCore;
using SGEDI.Application.DTOs;
using SGEDI.Application.Interfaces.Catalogos;
using SGEDI.Domain.Cifrado;
using SGEDI.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGEDI.Infrastructure.Services.Catalogos.Providers
{
    public abstract class BaseCatalogoProvider : ICatalogoProvider
    {
        protected readonly ApplicationDbContext _db;
        protected readonly ICifradoService _cifrado;

        protected BaseCatalogoProvider(ApplicationDbContext db, ICifradoService cifrado)
        {
            _db = db;
            _cifrado = cifrado;
        }

        public abstract string Nombre { get; }

        public abstract Task<List<CatalogoItemDTO>> ObtenerItemsAsync();

        protected async Task<List<CatalogoItemDTO>> ProyectarCatalogo<T>(
            IQueryable<T> query,
            System.Linq.Expressions.Expression<System.Func<T, int>> idSelector,
            System.Linq.Expressions.Expression<System.Func<T, string>> nombreSelector)
        {
            return await query.Select(x => new CatalogoItemDTO
            {
                Id = idSelector.Compile()(x).ToString(),
                Descripcion = nombreSelector.Compile()(x)
            }).ToListAsync()
            .ContinueWith(t => t.Result.Select(x => {
                x.Id = _cifrado.Encriptar(x.Id);
                return x;
            }).ToList());
        }
    }
}
