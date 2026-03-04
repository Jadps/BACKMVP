using System;
using System.Collections.Generic;
using System.Text;

namespace SGEDI.Application.DTOs
{
    public class UsuarioDTO
    {
        public string? Id { get; set; } 
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public string? Password { get; set; } 
        public int? RolId { get; set; }
        public string? NombreRol { get; set; }
    }
}
