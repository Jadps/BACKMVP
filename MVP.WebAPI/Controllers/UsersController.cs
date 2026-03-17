using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Application.Interfaces.Users;
using MVP.Domain.Constants;
using MVP.WebAPI.Extensions;
using System;
using System.Threading.Tasks;

namespace MVP.WebAPI.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController(
    IUserService service, 
    ICurrentTenantService currentTenantService) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10) 
    {
        var result = await service.GetPagedAsync(pageNumber, pageSize);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await service.GetByUidAsync(id);
        return result.ToActionResult();
    }
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userUidJson = currentTenantService.UserId;

        if (string.IsNullOrEmpty(userUidJson) || !Guid.TryParse(userUidJson, out Guid uid))
        {
            return ApplicationResult.Failure("Unauthorized or invalid user ID.", ErrorType.Unauthorized).ToActionResult();
        }

        var result = await service.GetByUidAsync(uid);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.GlobalAdmin + "," + AppRoles.TenantAdmin)]
    public async Task<IActionResult> Post(UserDto dto)
    {
        var result = await service.CreateAsync(dto);
        return result.ToActionResult();
    }

    [HttpPut]
    [Authorize(Roles = AppRoles.GlobalAdmin + "," + AppRoles.TenantAdmin)]
    public async Task<IActionResult> Put(UserDto dto)
    {
        var result = await service.UpdateAsync(dto);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AppRoles.GlobalAdmin + "," + AppRoles.TenantAdmin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await service.DeleteAsync(id);
        return result.ToActionResult();
    }
}
