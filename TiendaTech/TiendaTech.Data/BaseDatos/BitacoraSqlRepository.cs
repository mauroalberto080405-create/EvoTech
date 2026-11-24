using TiendaTech.Models;

namespace TiendaTech.Data;

// Guarda y lee la tabla Bitacora
public class BitacoraSqlRepository : IBitacoraRepository
{
    private readonly IConexionFactory _fabrica;
    public BitacoraSqlRepository(IConexionFactory fabrica)
    {
        _fabrica = fabrica;
        EsquemaBitacora.Asegurar(fabrica); // crea la tabla Bitacora si no existe
    }

    public void Agregar(RegistroBitacora b)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("INSERT INTO Bitacora (Fecha, Usuario, Evento) VALUES (@fecha, @usuario, @evento);")
            .Param("@fecha", b.Fecha.ToString(DbExtensiones.FormatoFecha))
            .Param("@usuario", b.Usuario)
            .Param("@evento", b.Evento);
        cmd.ExecuteNonQuery();
        b.Id = cn.UltimoId();
    }

    // Del inicio del día "desde" hasta el final del día "hasta", el más reciente primero
    public List<RegistroBitacora> ObtenerEnRango(DateTime desde, DateTime hasta)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("""
            SELECT Id, Fecha, Usuario, Evento FROM Bitacora
            WHERE Fecha >= @desde AND Fecha < @hasta
            ORDER BY Fecha DESC, Id DESC;
            """)
            .Param("@desde", desde.Date.ToString(DbExtensiones.FormatoFecha))
            .Param("@hasta", hasta.Date.AddDays(1).ToString(DbExtensiones.FormatoFecha));
        using var r = cmd.ExecuteReader();
        var lista = new List<RegistroBitacora>();
        while (r.Read())
            lista.Add(new RegistroBitacora
            {
                Id = r.Entero("Id"),
                Fecha = r.Fecha("Fecha"),
                Usuario = r.Texto("Usuario"),
                Evento = r.Texto("Evento")
            });
        return lista;
    }
}
