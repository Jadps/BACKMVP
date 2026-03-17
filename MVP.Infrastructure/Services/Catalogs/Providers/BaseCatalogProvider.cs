using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MVP.Application.DTOs;
using MVP.Application.Interfaces.Catalogs;

namespace MVP.Infrastructure.Services.Catalogs.Providers;

public abstract class BaseCatalogProvider : ICatalogProvider
{
    public abstract string Name { get; }
    public abstract Task<List<CatalogItemDto>> GetItemsAsync();

    protected async Task<List<CatalogItemDto>> ProjectCatalog<T>(
        IQueryable<T> query,
        Expression<Func<T, Guid>> idSelector,
        Expression<Func<T, string>> nameSelector)
    {
        var entities = await query.ToListAsync();
        var idFunc = idSelector.Compile();
        var nameFunc = nameSelector.Compile();

        return entities.Select(x => new CatalogItemDto
        {
            Id = idFunc(x),
            Description = nameFunc(x)
        }).ToList();
    }
}
