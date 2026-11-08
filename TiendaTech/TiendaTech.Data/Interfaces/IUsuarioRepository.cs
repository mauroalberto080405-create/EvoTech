using TiendaTech.Models;

namespace TiendaTech.Data;

public interface IUsuarioRepository
{
    Usuario? BuscarPorNombre(string nombreUsuario);
    List<Usuario> ObtenerTodos();
    void Agregar(Usuario usuario);
    void Eliminar(int id);
    void ActualizarClave(int id, string nuevaClave);
    bool TieneMovimientos(int id); // ventas o compras
}
