using Microsoft.EntityFrameworkCore;
using SGEDI.Application.DTOs;
using SGEDI.Domain.Cifrado;
using SGEDI.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGEDI.Infrastructure.Services.Catalogos.Providers
{
    public class RoleCatalogoProvider : BaseCatalogoProvider
    {
        public RoleCatalogoProvider(ApplicationDbContext db, ICifradoService cifrado) : base(db, cifrado) { }

        public override string Nombre => "roles";

        public override async Task<List<CatalogoItemDTO>> ObtenerItemsAsync()
        {
            return await ProyectarCatalogo(
                _db.Roles.Where(r => !r.Borrado),
                r => r.Id,
                r => r.Name ?? r.Descripcion ?? string.Empty
            );
        }
    }
}
