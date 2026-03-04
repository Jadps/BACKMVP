using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SGEDI.Application.DTOs;
using SGEDI.Domain.Cifrado;
using SGEDI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGEDI.Application.Interfaces.Usuarios;

namespace SGEDI.Application.Services.Usuarios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IMapper _mapper;
        private readonly ICifradoService _cifrado;

        public UsuarioService(UserManager<Usuario> userManager, IMapper mapper, ICifradoService cifrado)
        {
            _userManager = userManager;
            _mapper = mapper;
            _cifrado = cifrado;
        }

        public async Task<List<UsuarioDTO>> GetTodosAsync()
        {
            var usuarios = await _userManager.Users
                .Include(u => u.Rol)
                .Where(u => !u.Borrado)
                .ToListAsync();
                
            return _mapper.Map<List<UsuarioDTO>>(usuarios);
        }

        public async Task<IdentityResult> CrearAsync(UsuarioDTO dto)
        {
            var usuario = _mapper.Map<Usuario>(dto);
            usuario.UserName = dto.Email;
            
            var result = await _userManager.CreateAsync(usuario, dto.Password!);
            
            return result;
        }
    }
}
