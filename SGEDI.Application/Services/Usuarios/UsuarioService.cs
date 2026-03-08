using AutoMapper;
using SGEDI.Application.DTOs;
using SGEDI.Application.Exceptions;
using SGEDI.Application.Interfaces;
using SGEDI.Application.Interfaces.Usuarios;
using SGEDI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGEDI.Application.Services.Usuarios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IIdentityService _identityService;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly IMapper _mapper;

        public UsuarioService(
            IIdentityService identityService,
            ICurrentTenantService currentTenantService,
            IMapper mapper)
        {
            _identityService = identityService;
            _currentTenantService = currentTenantService;
            _mapper = mapper;
        }

        public async Task<List<UsuarioDTO>> GetTodosAsync()
        {
            var usuarios = await _identityService.GetUsuariosActivosAsync();

            var roleIds = usuarios
                .SelectMany(u => u.UserRoles)
                .Select(ur => ur.RoleId)
                .Distinct()
                .ToList();

            var roles = await _identityService.GetRolesByIdsAsync(roleIds);
            var rolesPorId = roles.ToDictionary(r => r.Id);

            return usuarios.Select(user =>
            {
                var dto = _mapper.Map<UsuarioDTO>(user);
                dto.NombreCompleto = user.NombreCompleto;
                dto.Roles = user.UserRoles
                    .Where(ur => rolesPorId.ContainsKey(ur.RoleId))
                    .Select(ur => _mapper.Map<RolDTO>(rolesPorId[ur.RoleId]))
                    .ToList();
                return dto;
            }).ToList();
        }

        public async Task<UsuarioDTO?> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty) return null;

            var user = await _identityService.GetUsuarioActivoByUidAsync(id);
            
            if (user == null) 
            {
                throw new NotFoundException("El usuario solicitado no fue encontrado.");
            }

            var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
            var roles = await _identityService.GetRolesByIdsAsync(roleIds);

            var dto = _mapper.Map<UsuarioDTO>(user);
            dto.Roles = roles.Select(r => _mapper.Map<RolDTO>(r)).ToList();

            return dto;
        }

        public async Task<ApplicationResult> CrearAsync(UsuarioDTO dto)
        {
            var usuario = _mapper.Map<Usuario>(dto);
            usuario.UserName = dto.Email;
            
            if (!_currentTenantService.IsSuperAdmin)
            {
                var currentTenantId = _currentTenantService.TenantId;
                if (currentTenantId.HasValue)
                {
                    usuario.TenantId = currentTenantId.Value;
                }
            }
            
            var nombresRolesNuevos = await ObtenerNombresRolesAsync(dto.Roles);
            
            return await _identityService.CrearUsuarioAsync(usuario, dto.Password!, nombresRolesNuevos);
        }

        public async Task<ApplicationResult> ActualizarAsync(UsuarioDTO dto)
        {
            if (dto.Id == null)
                throw new ValidationException("El identificador del usuario proporcionado es nulo.");

            var user = await _identityService.GetUsuarioActivoByUidAsync(dto.Id.Value);

            if (user == null)
            {
                throw new NotFoundException("El usuario que intentas actualizar no existe.");
            }

            _mapper.Map(dto, user);
            
            var nombresRolesNuevos = await ObtenerNombresRolesAsync(dto.Roles);

            return await _identityService.ActualizarUsuarioAsync(user, nombresRolesNuevos);
        }

        public async Task<bool> BorrarAsync(Guid id)
        {
            var success = await _identityService.BorrarUsuarioAsync(id);
            
            if (!success)
            {
                throw new NotFoundException("El usuario especificado para borrar no fue encontrado.");
            }

            return true;
        }

        private async Task<List<string>> ObtenerNombresRolesAsync(List<RolDTO> rolesDto)
        {
            var roleUidsToFind = new List<Guid>();
            foreach (var roleDto in rolesDto)
            {
                if (roleDto.Id.HasValue)
                {
                    roleUidsToFind.Add(roleDto.Id.Value);
                }
            }

            if (!roleUidsToFind.Any()) return new List<string>();

            var rolesDb = await _identityService.GetRolesByUidsAsync(roleUidsToFind);
            return rolesDb.Select(r => r.Name!).Where(n => !string.IsNullOrEmpty(n)).ToList();
        }
    }
}
