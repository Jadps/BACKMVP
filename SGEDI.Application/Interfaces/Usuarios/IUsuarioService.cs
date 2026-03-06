using Microsoft.AspNetCore.Identity;
using SGEDI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace SGEDI.Application.Interfaces.Usuarios
{
    public interface IUsuarioService
    {
        Task<List<UsuarioDTO>> GetTodosAsync();
        Task<UsuarioDTO?> GetByIdAsync(string idCifrado);
        Task<IdentityResult> CrearAsync(UsuarioDTO dto);
        Task<IdentityResult> ActualizarAsync(UsuarioDTO dto);
        Task<bool> BorrarAsync(string idCifrado);
    }
}
