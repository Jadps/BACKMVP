using MVP.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MVP.Application.Interfaces.Catalogos
{
    public interface ICatalogoService
    {
        Task<List<RolDTO>> GetRolesAsync();
        Task<ApplicationResult> CrearRolAsync(RolDTO dto);

        Task<List<ModuloDTO>> GetModulosMenuAsync();
    }
}
