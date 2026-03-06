using SGEDI.Domain.Cifrado;
using System.Security.Cryptography;
using System.Text;

public class CifradoService : ICifradoService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public CifradoService(CifradoOptions options)
    {
        var secret = options.SecretKey;

        if (string.IsNullOrEmpty(secret))
            throw new Exception("No hay SecretKey en la configuración");

        _key = Encoding.UTF8.GetBytes(secret.PadRight(32).Substring(0, 32));
        _iv  = Encoding.UTF8.GetBytes(secret.PadRight(16).Substring(0, 16));
    }

    public string Encriptar(string texto)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(texto);
        }

        return Convert.ToBase64String(ms.ToArray())
            .Replace('/', '-').Replace('+', '_').Replace("=", "");
    }

    public string Desencriptar(string textoCifrado)
    {
        textoCifrado = textoCifrado.Replace('-', '/').Replace('_', '+');
        switch (textoCifrado.Length % 4)
        {
            case 2: textoCifrado += "=="; break;
            case 3: textoCifrado += "=";  break;
        }

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(Convert.FromBase64String(textoCifrado));
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}