using MVP.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVP.Application.Interfaces.Catalogos
{
    public interface ICatalogoProvider
    {
        string Nombre { get; }
        Task<List<CatalogoItemDTO>> ObtenerItemsAsync();
    }
}
