using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using MVP.WebAPI.Extensions;
using System;
using System.Threading.Tasks;

namespace MVP.WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class TenantsController(ITenantService tenantService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = AppRoles.GlobalAdmin)]
    public async Task<IActionResult> Get() 
    {
        var result = await tenantService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await tenantService.GetByUidAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.GlobalAdmin)]
    public async Task<IActionResult> Post(TenantDto dto)
    {
        var result = await tenantService.CreateAsync(dto);
        return result.ToActionResult();
    }

    [HttpPut]
    [Authorize(Roles = AppRoles.GlobalAdmin)]
    public async Task<IActionResult> Put(TenantDto dto)
    {
        var result = await tenantService.UpdateAsync(dto);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AppRoles.GlobalAdmin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await tenantService.DeleteAsync(id);
        return result.ToActionResult();
    }
}
