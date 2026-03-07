using AutoMapper;
using SGEDI.Application.DTOs;
using SGEDI.Application.Exceptions;
using SGEDI.Application.Interfaces;
using SGEDI.Application.Interfaces.Usuarios;
using SGEDI.Domain.Cifrado;
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
        private readonly IMapper _mapper;
        private readonly ICifradoService _cifrado;

        public UsuarioService(
            IIdentityService identityService,
            IMapper mapper, 
            ICifradoService cifrado)
        {
            _identityService = identityService;
            _mapper = mapper;
            _cifrado = cifrado;
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

        public async Task<UsuarioDTO?> GetByIdAsync(string idCifrado)
        {
            if (string.IsNullOrEmpty(idCifrado)) return null;

            string decrypted = _cifrado.Desencriptar(idCifrado);
            if (!int.TryParse(decrypted, out int realId))
            {
                throw new ValidationException("El identificador del usuario proporcionado tiene un formato incorrecto.");
            }

            var user = await _identityService.GetUsuarioActivoByIdAsync(realId);
            
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
            
            var nombresRolesNuevos = await ObtenerNombresRolesAsync(dto.Roles);
            
            return await _identityService.CrearUsuarioAsync(usuario, dto.Password!, nombresRolesNuevos);
        }

        public async Task<ApplicationResult> ActualizarAsync(UsuarioDTO dto)
        {
            int realId = int.Parse(_cifrado.Desencriptar(dto.Id!));
            var user = await _identityService.GetUsuarioActivoByIdAsync(realId);

            if (user == null)
            {
                throw new NotFoundException("El usuario que intentas actualizar no existe.");
            }

            _mapper.Map(dto, user);
            
            var nombresRolesNuevos = await ObtenerNombresRolesAsync(dto.Roles);

            return await _identityService.ActualizarUsuarioAsync(user, nombresRolesNuevos);
        }

        public async Task<bool> BorrarAsync(string idCifrado)
        {
            int realId = int.Parse(_cifrado.Desencriptar(idCifrado));
            var success = await _identityService.BorrarUsuarioAsync(realId);
            
            if (!success)
            {
                throw new NotFoundException("El usuario especificado para borrar no fue encontrado.");
            }

            return true;
        }

        private async Task<List<string>> ObtenerNombresRolesAsync(List<RolDTO> rolesDto)
        {
            var roleIdsToFind = new List<int>();
            foreach (var roleDto in rolesDto)
            {
                if (roleDto.Id != null && int.TryParse(_cifrado.Desencriptar(roleDto.Id), out int roleId))
                {
                    roleIdsToFind.Add(roleId);
                }
            }

            if (!roleIdsToFind.Any()) return new List<string>();

            var rolesDb = await _identityService.GetRolesByIdsAsync(roleIdsToFind);
            return rolesDb.Select(r => r.Name!).Where(n => !string.IsNullOrEmpty(n)).ToList();
        }
    }
}
