using System;
using System.Collections.Generic;
using System.Text;

namespace SGEDI.Application.DTOs
{
    public class UsuarioDTO
    {
        public Guid? Id { get; set; } 
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public string? Password { get; set; } 
        public int TenantId { get; set; }
        public List<RolDTO> Roles { get; set; } = new();
        public string? NombreCompleto { get; set; }
    }
}
