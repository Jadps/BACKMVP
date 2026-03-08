using Microsoft.EntityFrameworkCore;
using MVP.Application.DTOs;
using MVP.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVP.Infrastructure.Services.Catalogos.Providers
{
    public class TenantCatalogoProvider : BaseCatalogoProvider
    {
        public TenantCatalogoProvider(ApplicationDbContext db) : base(db) { }

        public override string Nombre => "tenants";

        public override async Task<List<CatalogoItemDTO>> ObtenerItemsAsync()
        {
            return await ProyectarCatalogo(
                _db.Tenants.Where(t => !t.Borrado),
                t => t.Uid,
                t => t.Nombre
            );
        }
    }
}
