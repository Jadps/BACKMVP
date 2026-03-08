using Microsoft.EntityFrameworkCore;
using SGEDI.Application.DTOs;
using SGEDI.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGEDI.Infrastructure.Services.Catalogos.Providers
{
    public class UsuarioCatalogoProvider : BaseCatalogoProvider
    {
        public UsuarioCatalogoProvider(ApplicationDbContext db) : base(db) { }

        public override string Nombre => "usuarios";

        public override async Task<List<CatalogoItemDTO>> ObtenerItemsAsync()
        {
            return await ProyectarCatalogo(
                _db.Users.Where(u => !u.Borrado),
                u => u.Uid,
                u => (u.Nombre + " " + u.PrimerApellido + " " + u.SegundoApellido).Trim()
            );
        }
    }
}
