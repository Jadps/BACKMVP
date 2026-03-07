using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SGEDI.Application.DTOs;
using SGEDI.Domain.Cifrado;
using SGEDI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGEDI.Application.Interfaces.Usuarios;

namespace SGEDI.Application.Services.Usuarios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<Rol> _roleManager;
        private readonly IMapper _mapper;
        private readonly ICifradoService _cifrado;

        public UsuarioService(
            UserManager<Usuario> userManager, 
            RoleManager<Rol> roleManager,
            IMapper mapper, 
            ICifradoService cifrado)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _cifrado = cifrado;
        }

        public async Task<List<UsuarioDTO>> GetTodosAsync()
        {
            var usuariosQuery = _userManager.Users
                .Where(u => !u.Borrado);

            var usuarios = await usuariosQuery
                .Include(u => u.UserRoles)
                .ToListAsync();

            var roleIds = usuarios
                .SelectMany(u => u.UserRoles)
                .Select(ur => ur.RoleId)
                .Distinct()
                .ToList();

            var roles = await _roleManager.Roles
                .Where(r => roleIds.Contains(r.Id))
                .ToListAsync();

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

            try 
            {
                string decrypted = _cifrado.Desencriptar(idCifrado);
                if (!int.TryParse(decrypted, out int realId))
                {
                    return null;
                }

                var user = await _userManager.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.Id == realId && !u.Borrado);
                
                if (user == null) return null;

                var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
                var roles = await _roleManager.Roles
                    .Where(r => roleIds.Contains(r.Id))
                    .ToListAsync();

                var dto = _mapper.Map<UsuarioDTO>(user);
                dto.Roles = roles.Select(r => _mapper.Map<RolDTO>(r)).ToList();

                return dto;
            }
            catch (Exception) { return null; }
        }

        public async Task<IdentityResult> CrearAsync(UsuarioDTO dto)
        {
            var usuario = _mapper.Map<Usuario>(dto);
            usuario.UserName = dto.Email;
            
            var result = await _userManager.CreateAsync(usuario, dto.Password!);

            if (result.Succeeded && dto.Roles.Count > 0)
            {
                await SyncRolesAsync(usuario, dto.Roles);
            }
            
            return result;
        }

        public async Task<IdentityResult> ActualizarAsync(UsuarioDTO dto)
        {
            try 
            {
                int realId = int.Parse(_cifrado.Desencriptar(dto.Id!));
                var user = await _userManager.FindByIdAsync(realId.ToString());

                if (user == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado" });
                }

                _mapper.Map(dto, user);
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    await SyncRolesAsync(user, dto.Roles);
                }

                return result;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<bool> BorrarAsync(string idCifrado)
        {
            try 
            {
                int realId = int.Parse(_cifrado.Desencriptar(idCifrado));
                var user = await _userManager.FindByIdAsync(realId.ToString());

                if (user == null) return false;

                user.Borrado = true;
                await _userManager.UpdateAsync(user);
                return true;
            }
            catch { return false; }
        }

        private async Task SyncRolesAsync(Usuario user, List<RolDTO> rolesDto)
        {
            var rolesActuales = await _userManager.GetRolesAsync(user);
            
            if (rolesActuales.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, rolesActuales);
            }

            var nombresRolesNuevos = new List<string>();
            foreach (var roleDto in rolesDto)
            {
                if (roleDto.Id != null && int.TryParse(_cifrado.Desencriptar(roleDto.Id), out int roleId))
                {
                    var rol = await _roleManager.FindByIdAsync(roleId.ToString());
                    if (rol != null && rol.Name != null)
                    {
                        nombresRolesNuevos.Add(rol.Name);
                    }
                }
            }

            if (nombresRolesNuevos.Count > 0)
            {
                await _userManager.AddToRolesAsync(user, nombresRolesNuevos);
            }
        }
    }
}
