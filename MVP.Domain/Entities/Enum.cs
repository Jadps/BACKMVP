using System;
using System.Collections.Generic;
using System.Text;

namespace MVP.Domain.Entities
{
    public enum NivelPermiso
    {
        Ninguno = 0,
        Lectura = 1,      // Ver
        Escritura = 2,    // Ver + Crear/Editar
        Admin = 3         // Ver + Crear/Editar + SD
    }
}
