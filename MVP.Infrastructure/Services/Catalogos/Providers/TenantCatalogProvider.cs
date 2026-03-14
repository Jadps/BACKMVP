using Microsoft.EntityFrameworkCore;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVP.Infrastructure.Services.Catalogos.Providers;

public class TenantCatalogProvider(IApplicationDbContext context) : BaseCatalogProvider
{
    public override string Name => "Tenants";

    public override async Task<List<CatalogItemDto>> GetItemsAsync()
    {
        return await ProjectCatalog(
            context.Tenants.AsNoTracking().Where(t => !t.IsDeleted),
            t => t.Uid,
            t => t.Name);
    }
}
