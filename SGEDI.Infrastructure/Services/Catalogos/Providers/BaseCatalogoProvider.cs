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
            System.Func<T, int> idSelector,
            System.Func<T, string> nombreSelector)
        {
            var data = await query.ToListAsync();
            return data.Select(x => new CatalogoItemDTO(
                _cifrado.Encriptar(idSelector(x).ToString()),
                nombreSelector(x)
            )).ToList();
        }
    }
}
