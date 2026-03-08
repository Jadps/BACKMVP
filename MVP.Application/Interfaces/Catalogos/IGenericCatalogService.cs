using System.Collections.Generic;
using System.Threading.Tasks;
using MVP.Application.DTOs;

namespace MVP.Application.Interfaces.Catalogos;

public interface IGenericCatalogService
{
    Task<List<CatalogoItemDTO>> GetCatalogoAsync(string nombreCatalogo);
}
