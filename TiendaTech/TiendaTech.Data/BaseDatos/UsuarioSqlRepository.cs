using TiendaTech.Models;

namespace TiendaTech.Data;

public class UsuarioSqlRepository : IUsuarioRepository
{
    private readonly IConexionFactory _fabrica;
    public UsuarioSqlRepository(IConexionFactory fabrica) => _fabrica = fabrica;

    public Usuario? BuscarPorNombre(string nombreUsuario)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("""
            SELECT Id, NombreUsuario, Clave, Rol FROM Usuario
            WHERE LOWER(NombreUsuario) = LOWER(@usuario);
            """).Param("@usuario", nombreUsuario);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new Usuario
        {
            Id = r.Entero("Id"),
            NombreUsuario = r.Texto("NombreUsuario"),
            Clave = r.Texto("Clave"),
            Rol = r.Texto("Rol")
        };
    }

    public List<Usuario> ObtenerTodos()
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("SELECT Id, NombreUsuario, Clave, Rol FROM Usuario ORDER BY Id;");
        using var r = cmd.ExecuteReader();
        var lista = new List<Usuario>();
        while (r.Read())
            lista.Add(new Usuario
            {
                Id = r.Entero("Id"),
                NombreUsuario = r.Texto("NombreUsuario"),
                Clave = r.Texto("Clave"),
                Rol = r.Texto("Rol")
            });
        return lista;
    }

    public void Agregar(Usuario u)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("INSERT INTO Usuario (NombreUsuario, Clave, Rol) VALUES (@usuario, @clave, @rol);")
            .Param("@usuario", u.NombreUsuario).Param("@clave", u.Clave).Param("@rol", u.Rol);
        cmd.ExecuteNonQuery();
        u.Id = cn.UltimoId();
    }

    public void ActualizarClave(int id, string nuevaClave)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("UPDATE Usuario SET Clave = @clave WHERE Id = @id;")
            .Param("@clave", nuevaClave).Param("@id", id);
        cmd.ExecuteNonQuery();
    }

    public void Eliminar(int id)
    {
        using var cn = _fabrica.Abrir();
        cn.EliminarPorId("Usuario", id);
    }

    public bool TieneMovimientos(int id)
    {
        using var cn = _fabrica.Abrir();
        return cn.Contar("""
            SELECT (SELECT COUNT(*) FROM Venta WHERE UsuarioId = @id)
                 + (SELECT COUNT(*) FROM Compra WHERE UsuarioId = @id);
            """, id) > 0;
    }
}
