namespace TiendaTech.Data;

// Abstraccion para guardar y comprobar claves sin almacenarlas en texto plano (DIP)
public interface IHasheadorClave
{
    // Devuelve el hash con sal listo para guardar en la tabla Usuario
    string Hashear(string clave);

    // true si la clave escrita corresponde al hash guardado
    bool Verificar(string clave, string hashGuardado);

    // true si el texto ya tiene formato de hash (sirve para migrar claves antiguas)
    bool EsHash(string valor);
}
