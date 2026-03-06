using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SGEDI.Application.DTOs;
using SGEDI.Application.Interfaces.Catalogos;
using SGEDI.Domain.Cifrado;
using SGEDI.Domain.Entities;
using SGEDI.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SGEDI.Application.Services.Catalogos
{
    public class CatalogoService : ICatalogoService
    {
        private readonly RoleManager<Rol> _roleManager;
        private readonly IMapper _mapper;
        private readonly ICifradoService _cifrado;
        private readonly IRepository<Modulo> _moduloRepository;

        public CatalogoService(
            RoleManager<Rol> roleManager, 
            IMapper mapper, 
            ICifradoService cifrado,
            IRepository<Modulo> moduloRepository)
        {
            _roleManager = roleManager;
            _mapper = mapper;
            _cifrado = cifrado;
            _moduloRepository = moduloRepository;
        }

        public async Task<List<RolDTO>> GetRolesAsync()
        {
            var roles = await _roleManager.Roles.Where(r => !r.Borrado).ToListAsync();
            return _mapper.Map<List<RolDTO>>(roles);
        }

        public async Task CrearRolAsync(RolDTO dto)
        {
            var rol = new Rol
            {
                Name = dto.Name,
                Descripcion = dto.Descripcion,
                Borrado = false
            };

            var result = await _roleManager.CreateAsync(rol);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Error al crear el rol: {errors}");
            }
        }

        public async Task<List<ModuloDTO>> GetModulosMenuAsync()
        {
            var modulos = await _moduloRepository.GetAllAsync(
                filter: m => m.PadreId == null,
                includeProperties: "SubModulos"
            );

            return _mapper.Map<List<ModuloDTO>>(modulos);
        }
    }
}
