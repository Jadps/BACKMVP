using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MVP.Application.DTOs;
using MVP.Application.Interfaces.Catalogs;

namespace MVP.Infrastructure.Services.Catalogs;

public class GenericCatalogService(IEnumerable<ICatalogProvider> providers) : IGenericCatalogService
{
    private readonly IEnumerable<ICatalogProvider> _providers = providers;

    public async Task<List<CatalogItemDto>> GetCatalogAsync(string catalogName)
    {
        var provider = _providers.FirstOrDefault(p => p.Name.Equals(catalogName, StringComparison.OrdinalIgnoreCase));

        if (provider == null)
            throw new Exception($"Catalog '{catalogName}' not found.");

        return await provider.GetItemsAsync();
    }
}
