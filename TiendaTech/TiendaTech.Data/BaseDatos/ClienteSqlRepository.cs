using TiendaTech.Models;

namespace TiendaTech.Data;

public class ClienteSqlRepository : IClienteRepository
{
    private readonly IConexionFactory _fabrica;
    public ClienteSqlRepository(IConexionFactory fabrica) => _fabrica = fabrica;

    public List<Cliente> ObtenerTodos()
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("SELECT Id, Nombre, Documento, Telefono FROM Cliente ORDER BY Id;");
        using var r = cmd.ExecuteReader();
        var lista = new List<Cliente>();
        while (r.Read())
            lista.Add(new Cliente
            {
                Id = r.Entero("Id"),
                Nombre = r.Texto("Nombre"),
                Documento = r.Texto("Documento"),
                Telefono = r.Texto("Telefono")
            });
        return lista;
    }

    public void Agregar(Cliente c)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("INSERT INTO Cliente (Nombre, Documento, Telefono) VALUES (@nombre, @doc, @tel);")
            .Param("@nombre", c.Nombre).Param("@doc", c.Documento).Param("@tel", c.Telefono);
        cmd.ExecuteNonQuery();
        c.Id = cn.UltimoId();
    }

    public bool ExisteDocumento(string documento)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("SELECT COUNT(*) FROM Cliente WHERE Documento = @doc;").Param("@doc", documento);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public void Eliminar(int id)
    {
        using var cn = _fabrica.Abrir();
        cn.EliminarPorId("Cliente", id);
    }

    public bool TieneVentas(int id)
    {
        using var cn = _fabrica.Abrir();
        return cn.Contar("SELECT COUNT(*) FROM Venta WHERE ClienteId = @id;", id) > 0;
    }
}
