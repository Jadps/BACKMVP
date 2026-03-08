using Microsoft.EntityFrameworkCore;
using MVP.Application.DTOs;
using MVP.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVP.Infrastructure.Services.Catalogos.Providers
{
    public class RoleCatalogoProvider : BaseCatalogoProvider
    {
        public RoleCatalogoProvider(ApplicationDbContext db) : base(db) { }

        public override string Nombre => "roles";

        public override async Task<List<CatalogoItemDTO>> ObtenerItemsAsync()
        {
            return await ProyectarCatalogo(
                _db.Roles.Where(r => !r.Borrado),
                r => r.Uid,
                r => r.Name ?? r.Descripcion ?? string.Empty
            );
        }
    }
}
