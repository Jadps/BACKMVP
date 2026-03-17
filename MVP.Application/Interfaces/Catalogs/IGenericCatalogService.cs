using System.Collections.Generic;
using System.Threading.Tasks;
using MVP.Application.DTOs;

namespace MVP.Application.Interfaces.Catalogs;

public interface IGenericCatalogService
{
    Task<List<CatalogItemDto>> GetCatalogAsync(string catalogName);
}
