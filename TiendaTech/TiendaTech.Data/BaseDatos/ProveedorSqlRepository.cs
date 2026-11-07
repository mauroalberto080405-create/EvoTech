using TiendaTech.Models;

namespace TiendaTech.Data;

public class ProveedorSqlRepository : IProveedorRepository
{
    private readonly IConexionFactory _fabrica;
    public ProveedorSqlRepository(IConexionFactory fabrica) => _fabrica = fabrica;

    public List<Proveedor> ObtenerTodos()
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("SELECT Id, Empresa, Ruc, Telefono FROM Proveedor ORDER BY Id;");
        using var r = cmd.ExecuteReader();
        var lista = new List<Proveedor>();
        while (r.Read())
            lista.Add(new Proveedor
            {
                Id = r.Entero("Id"),
                Empresa = r.Texto("Empresa"),
                Ruc = r.Texto("Ruc"),
                Telefono = r.Texto("Telefono")
            });
        return lista;
    }

    public void Agregar(Proveedor p)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("INSERT INTO Proveedor (Empresa, Ruc, Telefono) VALUES (@empresa, @ruc, @tel);")
            .Param("@empresa", p.Empresa).Param("@ruc", p.Ruc).Param("@tel", p.Telefono);
        cmd.ExecuteNonQuery();
        p.Id = cn.UltimoId();
    }

    public bool ExisteRuc(string ruc)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("SELECT COUNT(*) FROM Proveedor WHERE Ruc = @ruc;").Param("@ruc", ruc);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public void Eliminar(int id)
    {
        using var cn = _fabrica.Abrir();
        cn.EliminarPorId("Proveedor", id);
    }

    public bool TieneMovimientos(int id)
    {
        using var cn = _fabrica.Abrir();
        return cn.Contar("""
            SELECT (SELECT COUNT(*) FROM Producto WHERE ProveedorId = @id)
                 + (SELECT COUNT(*) FROM Compra WHERE ProveedorId = @id);
            """, id) > 0;
    }
}
