using Microsoft.EntityFrameworkCore;
using MVP.Application.DTOs;
using MVP.Application.Interfaces.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVP.Infrastructure.Services.Catalogs.Providers;

public class TenantCatalogProvider(ITenantRepository tenantRepository) : BaseCatalogProvider
{
    public override string Name => "Tenants";

    public override async Task<List<CatalogItemDto>> GetItemsAsync()
    {
        var tenants = await tenantRepository.GetAllActiveAsync();
        return tenants.Select(t => new CatalogItemDto
        {
            Id = t.Uid,
            Description = t.Name
        }).ToList();
    }
}
