using AutoMapper;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Application.Interfaces.Usuarios;
using MVP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MVP.Application.Services.Usuarios;

public class UsuarioService(
    IIdentityService identityService,
    ICurrentTenantService currentTenantService,
    IApplicationDbContext context,
    IMapper mapper) : IUsuarioService
{
    public async Task<List<UsuarioDTO>> GetTodosAsync()
    {
        var usuarios = await identityService.GetUsuariosActivosAsync();
        var todosLosRoles = await identityService.GetRolesActivosAsync();
        var rolesPorId = todosLosRoles.ToDictionary(r => r.Id);

        return usuarios.Select(user =>
        {
            var dto = mapper.Map<UsuarioDTO>(user);
            var roles = (user.RoleIds ?? new List<int>())
                .Where(id => rolesPorId.ContainsKey(id))
                .Select(id => mapper.Map<RolDTO>(rolesPorId[id]))
                .ToList();
            
            dto.NombreCompleto = user.NombreCompleto;
            dto.Roles = roles;
            return dto;
        }).ToList();
    }

    public async Task<PagedResult<UsuarioDTO>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var result = await identityService.GetUsuariosActivosPagedAsync(pageNumber, pageSize);
        var todosLosRoles = await identityService.GetRolesActivosAsync();
        var rolesPorId = todosLosRoles.ToDictionary(r => r.Id);

        var dtos = result.Items.Select(user =>
        {
            var dto = mapper.Map<UsuarioDTO>(user);
            var roles = (user.RoleIds ?? new List<int>())
                .Where(id => rolesPorId.ContainsKey(id))
                .Select(id => mapper.Map<RolDTO>(rolesPorId[id]))
                .ToList();
            
            dto.NombreCompleto = user.NombreCompleto;
            dto.Roles = roles;
            return dto;
        }).ToList();
        
        return new PagedResult<UsuarioDTO>(dtos, result.TotalCount, pageNumber, pageSize);
    }

    public async Task<ApplicationResult<UsuarioDTO>> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty) 
            return ApplicationResult<UsuarioDTO>.Failure("Id de usuario no válido.", ErrorType.Validation);

        var user = await identityService.GetUsuarioActivoByUidAsync(id);
        
        if (user == null) 
            return ApplicationResult<UsuarioDTO>.Failure("El usuario solicitado no fue encontrado.", ErrorType.NotFound);

        var roles = await identityService.GetRolesByIdsAsync(user.RoleIds ?? new List<int>());
        var dto = mapper.Map<UsuarioDTO>(user);
        var rolesDto = roles.Select(r => mapper.Map<RolDTO>(r)).ToList();

        dto.Roles = rolesDto;
        return ApplicationResult<UsuarioDTO>.Success(dto);
    }

    public async Task<ApplicationResult<UsuarioDTO>> GetPerfilActualAsync()
    {
        var userIdStr = currentTenantService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return ApplicationResult<UsuarioDTO>.Failure("No se pudo identificar el perfil actual.", ErrorType.Unauthorized);

        return await GetByIdAsync(userId);
    }

    public async Task<ApplicationResult> CrearAsync(UsuarioDTO dto)
    {
        var usuario = mapper.Map<Usuario>(dto);
        usuario.UserName = dto.Email;
        
        if (dto.TenantId.HasValue)
        {
            var tenant = await context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Uid == dto.TenantId.Value);
            if (tenant == null) return ApplicationResult.Failure("El Tenant proporcionado no existe.", ErrorType.NotFound);
            usuario.TenantId = tenant.Id;
        }
        else if (!currentTenantService.IsSuperAdmin)
        {
            usuario.TenantId = currentTenantService.TenantId;
        }
        
        var nombresRolesNuevos = await ObtenerNombresRolesAsync(dto.Roles);
        return await identityService.CrearUsuarioAsync(usuario, dto.Password!, nombresRolesNuevos);
    }

    public async Task<ApplicationResult> ActualizarAsync(UsuarioDTO dto)
    {
        if (dto.Id == null)
            return ApplicationResult.Failure("El identificador del usuario proporcionado es nulo.", ErrorType.Validation);

        var user = await identityService.GetUsuarioActivoByUidAsync(dto.Id.Value);
        if (user == null)
            return ApplicationResult.Failure("El usuario que intentas actualizar no existe.", ErrorType.NotFound);

        mapper.Map(dto, user);
        
        if (dto.TenantId.HasValue)
        {
            var tenant = await context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Uid == dto.TenantId.Value);
            if (tenant == null) return ApplicationResult.Failure("El Tenant proporcionado no existe.", ErrorType.NotFound);
            user.TenantId = tenant.Id;
        }

        var nombresRolesNuevos = await ObtenerNombresRolesAsync(dto.Roles);
        return await identityService.ActualizarUsuarioAsync(user, nombresRolesNuevos);
    }

    public async Task<ApplicationResult> BorrarAsync(Guid id)
    {
        return await identityService.BorrarUsuarioAsync(id);
    }

    private async Task<List<string>> ObtenerNombresRolesAsync(List<RolDTO> rolesDto)
    {
        var roleUidsToFind = rolesDto
            .Where(r => r.Id.HasValue)
            .Select(r => r.Id!.Value)
            .ToList();

        if (!roleUidsToFind.Any()) return new List<string>();

        var rolesDb = await identityService.GetRolesByUidsAsync(roleUidsToFind);
        return rolesDb.Select(r => r.Name!).Where(n => !string.IsNullOrEmpty(n)).ToList();
    }
}
