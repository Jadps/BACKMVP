using SGEDI.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGEDI.Application.Interfaces.Catalogos
{
    public interface ICatalogoProvider
    {
        string Nombre { get; }
        Task<List<CatalogoItemDTO>> ObtenerItemsAsync();
    }
}
