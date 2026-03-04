using SGEDI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace SGEDI.Application.Interfaces.Catalogos
{
    public interface ICatalogoService
    {
        Task<List<RolDTO>> GetRolesAsync();
        Task CrearRolAsync(RolDTO dto);

        Task<List<ModuloDTO>> GetModulosMenuAsync();
    }
}
