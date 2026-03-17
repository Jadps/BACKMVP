using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVP.Infrastructure.Services.Catalogs.Providers;

public class UserCatalogProvider(IIdentityService identityService) : BaseCatalogProvider
{
    public override string Name => "Users";

    public override async Task<List<CatalogItemDto>> GetItemsAsync()
    {
        var users = await identityService.GetActiveUsersAsync();
        return users.Select(u => new CatalogItemDto
        {
            Id = u.Uid,
            Description = u.FullName,
            Additional = u.Email
        }).ToList();
    }
}
