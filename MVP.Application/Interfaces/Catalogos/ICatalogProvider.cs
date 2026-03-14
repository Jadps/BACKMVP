using System.Collections.Generic;
using System.Threading.Tasks;
using MVP.Application.DTOs;

namespace MVP.Application.Interfaces.Catalogos;

public interface ICatalogProvider
{
    string Name { get; }
    Task<List<CatalogItemDto>> GetItemsAsync();
}
