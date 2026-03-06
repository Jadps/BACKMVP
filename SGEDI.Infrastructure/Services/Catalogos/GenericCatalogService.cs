using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGEDI.Application.DTOs;
using SGEDI.Application.Interfaces.Catalogos;

namespace SGEDI.Infrastructure.Services.Catalogos;

public class GenericCatalogService : IGenericCatalogService
{
    private readonly IEnumerable<ICatalogoProvider> _providers;

    public GenericCatalogService(IEnumerable<ICatalogoProvider> providers)
    {
        _providers = providers;
    }

    public async Task<List<CatalogoItemDTO>> GetCatalogoAsync(string nombreCatalogo)
    {
        var provider = _providers.FirstOrDefault(p => p.Nombre.Equals(nombreCatalogo, StringComparison.OrdinalIgnoreCase));

        if (provider == null)
            throw new Exception($"El catálogo '{nombreCatalogo}' no fue encontrado.");

        return await provider.ObtenerItemsAsync();
    }
}

