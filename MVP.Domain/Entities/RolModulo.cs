using System;
using System.Collections.Generic;
using System.Text;

namespace MVP.Domain.Entities
{
    public class RolModulo
    {
        public int RolId { get; set; }
        public virtual Rol Rol { get; set; } = null!;

        public int ModuloId { get; set; }
        public virtual Modulo Modulo { get; set; } = null!;

        public NivelPermiso Permiso { get; set; }
    }
}
