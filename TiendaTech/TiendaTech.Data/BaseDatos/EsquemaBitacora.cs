namespace TiendaTech.Data;

// Crea la tabla Bitacora (registro de intentos de inicio de sesión) si todavía no existe.
// - BD nueva o antigua: solo crea la tabla que falta, SIN borrar datos.
// Está separado de DbInicializador para que esta mejora no toque archivos de otros módulos.
// Nunca se guarda la clave escrita: solo fecha, usuario escrito y evento.
public static class EsquemaBitacora
{
    public static void Asegurar(IConexionFactory fabrica)
    {
        using var cn = fabrica.Abrir();
        using var cmd = cn.Comando("""
            CREATE TABLE IF NOT EXISTS Bitacora (
                Id      INTEGER PRIMARY KEY AUTOINCREMENT,
                Fecha   TEXT NOT NULL,
                Usuario TEXT NOT NULL,
                Evento  TEXT NOT NULL
            );
            """);
        cmd.ExecuteNonQuery();
    }
}
