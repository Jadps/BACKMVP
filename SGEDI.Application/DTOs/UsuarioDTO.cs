using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGEDI.Application.DTOs
{
    public class UsuarioDTO
    {
        public Guid? Id { get; set; } 
        
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string PrimerApellido { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? SegundoApellido { get; set; }
        
        [MaxLength(100)]
        public string? Password { get; set; } 
        
        public int TenantId { get; set; }
        public List<RolDTO> Roles { get; set; } = new();
        public string? NombreCompleto { get; set; }
    }
}
