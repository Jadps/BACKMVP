using Microsoft.EntityFrameworkCore;
using SGEDI.Application.DTOs;
using SGEDI.Domain.Cifrado;
using SGEDI.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGEDI.Infrastructure.Services.Catalogos.Providers
{
    public class UsuarioCatalogoProvider : BaseCatalogoProvider
    {
        public UsuarioCatalogoProvider(ApplicationDbContext db, ICifradoService cifrado) : base(db, cifrado) { }

        public override string Nombre => "usuarios";

        public override async Task<List<CatalogoItemDTO>> ObtenerItemsAsync()
        {
            return await ProyectarCatalogo(
                _db.Users.Where(u => !u.Borrado),
                u => u.Id,
                u => (u.Nombre + " " + u.PrimerApellido + " " + u.SegundoApellido).Trim()
            );
        }
    }
}
