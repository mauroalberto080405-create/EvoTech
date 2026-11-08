using System.Security.Cryptography;

namespace TiendaTech.Data;

// PBKDF2-SHA256 con sal aleatoria por clave.
// Formato guardado: pbkdf2$iteraciones$sal(base64)$hash(base64)
public class HasheadorPbkdf2 : IHasheadorClave
{
    private const string Prefijo = "pbkdf2";
    private const int Iteraciones = 100_000;
    private const int TamSal = 16;
    private const int TamHash = 32;

    public string Hashear(string clave)
    {
        var sal = RandomNumberGenerator.GetBytes(TamSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(clave, sal, Iteraciones, HashAlgorithmName.SHA256, TamHash);
        return $"{Prefijo}${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string clave, string hashGuardado)
    {
        if (!EsHash(hashGuardado)) return false;
        try
        {
            var partes = hashGuardado.Split('$');
            var iteraciones = int.Parse(partes[1]);
            var sal = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);
            var calculado = Rfc2898DeriveBytes.Pbkdf2(clave, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(calculado, esperado); // comparacion en tiempo constante
        }
        catch (FormatException) { return false; }
        catch (OverflowException) { return false; }
    }

    public bool EsHash(string valor)
    {
        if (string.IsNullOrEmpty(valor)) return false;
        var partes = valor.Split('$');
        return partes.Length == 4 && partes[0] == Prefijo && int.TryParse(partes[1], out var n) && n > 0;
    }
}
