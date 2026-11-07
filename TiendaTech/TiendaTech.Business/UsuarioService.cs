using TiendaTech.Data;
using TiendaTech.Models;

namespace TiendaTech.Business;

public interface IUsuarioService
{
    List<Usuario> Listar();
    void Registrar(Usuario usuario);
    void Eliminar(Usuario? usuario);
}

public class UsuarioService : IUsuarioService
{
    public static readonly string[] Roles = { "Vendedor", "Administrador" };

    private readonly IUsuarioRepository _repo;
    private readonly ISesion _sesion;

    public UsuarioService(IUsuarioRepository repo, ISesion sesion)
    {
        _repo = repo;
        _sesion = sesion;
    }

    public List<Usuario> Listar() => _repo.ObtenerTodos();

    public void Registrar(Usuario u)
    {
        if (!Validaciones.CredencialValida(u.NombreUsuario) || !Validaciones.CredencialValida(u.Clave))
            throw new ArgumentException(Validaciones.MensajeCredencial);
        if (u.NombreUsuario.Length < 3) throw new ArgumentException("El usuario debe tener al menos 3 caracteres.");
        if (u.Clave.Length < 4) throw new ArgumentException("La clave debe tener al menos 4 caracteres.");
        if (!Roles.Contains(u.Rol)) throw new ArgumentException("Seleccione un rol válido.");
        if (_repo.BuscarPorNombre(u.NombreUsuario) != null) throw new ArgumentException("Ese nombre de usuario ya existe.");
        _repo.Agregar(u);
    }

    public void Eliminar(Usuario? u)
    {
        if (u == null) throw new ArgumentException("Seleccione un usuario de la tabla.");
        if (u.Id == _sesion.UsuarioActual?.Id) throw new ArgumentException("No puede eliminar su propio usuario.");
        if (u.Rol == "Administrador" && _repo.ObtenerTodos().Count(x => x.Rol == "Administrador") <= 1)
            throw new ArgumentException("Debe quedar al menos un administrador.");
        if (_repo.TieneMovimientos(u.Id))
            throw new ArgumentException($"No se puede eliminar a {u.NombreUsuario}: tiene ventas o compras registradas.");
        _repo.Eliminar(u.Id);
    }
}
