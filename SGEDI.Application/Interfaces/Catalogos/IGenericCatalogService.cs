using System.Collections.Generic;
using System.Threading.Tasks;
using SGEDI.Application.DTOs;

namespace SGEDI.Application.Interfaces.Catalogos;

public interface IGenericCatalogService
{
    Task<List<CatalogoItemDTO>> GetCatalogoAsync(string nombreCatalogo);
}
