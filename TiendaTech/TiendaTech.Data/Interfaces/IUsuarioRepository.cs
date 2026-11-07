using TiendaTech.Models;

namespace TiendaTech.Data;

public interface IUsuarioRepository
{
    Usuario? BuscarPorNombre(string nombreUsuario);
    List<Usuario> ObtenerTodos();
    void Agregar(Usuario usuario);
    void Eliminar(int id);
    bool TieneMovimientos(int id); // ventas o compras
}
