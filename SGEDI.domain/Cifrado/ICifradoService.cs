using System;
using System.Collections.Generic;
using System.Text;

namespace SGEDI.Domain.Cifrado
{
    public interface ICifradoService
    {
        string Encriptar(string texto);
        string Desencriptar(string textoCifrado);
    }
}
