using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVP.Infrastructure.Services.Catalogs.Providers;

public class RoleCatalogProvider(IIdentityService identityService) : BaseCatalogProvider
{
    public override string Name => "Roles";

    public override async Task<List<CatalogItemDto>> GetItemsAsync()
    {
        var roles = await identityService.GetActiveRolesAsync();
        return roles.Select(r => new CatalogItemDto
        {
            Id = r.Uid,
            Description = r.Name ?? string.Empty,
            Additional = r.Description
        }).ToList();
    }
}
