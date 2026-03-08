using System;
using System.Collections.Generic;
using System.Text;
using System.IO; 
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SGEDI.Application.Interfaces;

namespace SGEDI.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private class DesignTimeCurrentTenantService : ICurrentTenantService
    {
        public int? TenantId => null;
        public string? UserId => null;
        public void SetTenantId(int tenantId) { }
    }

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "../SGEDI.WebAPI/appsettings.Development.json"))
            .Build();

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        builder.UseNpgsql(connectionString);

        return new ApplicationDbContext(builder.Options, new DesignTimeCurrentTenantService());
    }
}