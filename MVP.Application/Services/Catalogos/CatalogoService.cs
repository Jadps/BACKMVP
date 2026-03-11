using AutoMapper;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Application.Interfaces.Catalogos;
using MVP.Domain.Entities;
using MVP.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace MVP.Application.Services.Catalogos
{
    public class CatalogoService : ICatalogoService
    {
        private readonly IIdentityService _identityService;
        private readonly IMapper _mapper;
        private readonly IRepository<Modulo> _moduloRepository;
        private readonly IMemoryCache _cache;
        private const string RolesCacheKey = "roles_cache";
        private const string ModulosCacheKey = "modulos_cache";

        public CatalogoService(
            IIdentityService identityService,
            IMapper mapper, 
            IRepository<Modulo> moduloRepository,
            IMemoryCache cache)
        {
            _identityService = identityService;
            _mapper = mapper;
            _moduloRepository = moduloRepository;
            _cache = cache;
        }

        public async Task<List<RolDTO>> GetRolesAsync()
        {
            if (!_cache.TryGetValue(RolesCacheKey, out List<RolDTO>? cachedRoles) || cachedRoles == null)
            {
                var roles = await _identityService.GetRolesActivosAsync();
                cachedRoles = _mapper.Map<List<RolDTO>>(roles);
                _cache.Set(RolesCacheKey, cachedRoles, TimeSpan.FromHours(1));
            }
            return cachedRoles;
        }

        public async Task CrearRolAsync(RolDTO dto)
        {
            var rol = new Rol
            {
                Name = dto.Name,
                Descripcion = dto.Descripcion,
                Borrado = false
            };

            var result = await _identityService.CrearRolAsync(rol);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors));
            }
            _cache.Remove(RolesCacheKey);
        }

        public async Task<List<ModuloDTO>> GetModulosMenuAsync()
        {
            if (!_cache.TryGetValue(ModulosCacheKey, out List<ModuloDTO>? cachedModulos) || cachedModulos == null)
            {
                var modulos = await _moduloRepository.GetAllAsync(
                    filter: m => m.PadreId == null,
                    disableTracking: true, 
                    m => m.SubModulos 
                );

                cachedModulos = _mapper.Map<List<ModuloDTO>>(modulos);
                _cache.Set(ModulosCacheKey, cachedModulos, TimeSpan.FromHours(1));
            }

            return cachedModulos;
        }
    }
}

