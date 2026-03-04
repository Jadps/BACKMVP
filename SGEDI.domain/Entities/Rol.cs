using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Identity;

namespace SGEDI.Domain.Entities;

public class Rol : IdentityRole
{
    // IdentityRole ya trae el Id (string) y Name
    // Aquí puedes agregar campos extras si tu SQL los tenía
    public string? Descripcion { get; set; }
    public bool Borrado { get; set; }
}
