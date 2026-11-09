using TiendaTech.Data;
using TiendaTech.Models;

namespace TiendaTech.Business;

public interface IUsuarioService
{
    List<Usuario> Listar();
    void Registrar(Usuario usuario);
    void Eliminar(Usuario? usuario);
    // Cambia la clave del usuario que tiene la sesión abierta (cualquier rol)
    void CambiarClave(string claveActual, string claveNueva, string confirmacion);
}

public class UsuarioService : IUsuarioService
{
    public const string RolAdministrador = "Administrador";
    public static readonly string[] Roles = { "Vendedor", RolAdministrador };

    private readonly IUsuarioRepository _repo;
    private readonly ISesion _sesion;
    private readonly IHasheadorClave _hasheador;

    public UsuarioService(IUsuarioRepository repo, ISesion sesion, IHasheadorClave hasheador)
    {
        _repo = repo;
        _sesion = sesion;
        _hasheador = hasheador;
    }

    // Validación de rol EN EL SERVICIO: aunque alguien llame al servicio sin pasar por
    // el menú (que ya deshabilita el botón), un no administrador no puede administrar usuarios.
    private void ExigirAdministrador()
    {
        if (_sesion.UsuarioActual == null)
            throw new UnauthorizedAccessException("Debe iniciar sesión para realizar esta acción.");
        if (_sesion.UsuarioActual.Rol != RolAdministrador)
            throw new UnauthorizedAccessException("Solo un administrador puede gestionar usuarios.");
    }

    public List<Usuario> Listar()
    {
        ExigirAdministrador();
        return _repo.ObtenerTodos();
    }

    public void Registrar(Usuario u)
    {
        ExigirAdministrador();
        if (!Validaciones.CredencialValida(u.NombreUsuario) || !Validaciones.CredencialValida(u.Clave))
            throw new ArgumentException(Validaciones.MensajeCredencial);
        if (u.NombreUsuario.Length < 3) throw new ArgumentException("El usuario debe tener al menos 3 caracteres.");
        if (u.Clave.Length < 4) throw new ArgumentException("La clave debe tener al menos 4 caracteres.");
        if (!Roles.Contains(u.Rol)) throw new ArgumentException("Seleccione un rol válido.");
        if (_repo.BuscarPorNombre(u.NombreUsuario) != null) throw new ArgumentException("Ese nombre de usuario ya existe.");
        // Se guarda el hash, nunca la clave escrita
        u.Clave = _hasheador.Hashear(u.Clave);
        _repo.Agregar(u);
    }

    public void Eliminar(Usuario? u)
    {
        ExigirAdministrador();
        if (u == null) throw new ArgumentException("Seleccione un usuario de la tabla.");
        if (u.Id == _sesion.UsuarioActual?.Id) throw new ArgumentException("No puede eliminar su propio usuario.");
        if (u.Rol == RolAdministrador && _repo.ObtenerTodos().Count(x => x.Rol == RolAdministrador) <= 1)
            throw new ArgumentException("Debe quedar al menos un administrador.");
        if (_repo.TieneMovimientos(u.Id))
            throw new ArgumentException($"No se puede eliminar a {u.NombreUsuario}: tiene ventas o compras registradas.");
        _repo.Eliminar(u.Id);
    }

    public void CambiarClave(string claveActual, string claveNueva, string confirmacion)
    {
        var actual = _sesion.UsuarioActual
            ?? throw new UnauthorizedAccessException("Debe iniciar sesión para cambiar la clave.");

        if (string.IsNullOrEmpty(claveActual) || string.IsNullOrEmpty(claveNueva) || string.IsNullOrEmpty(confirmacion))
            throw new ArgumentException("Complete la clave actual, la nueva y su confirmación.");

        // La clave actual se verifica contra el HASH guardado en la BD (no contra la copia de la sesión)
        var enBd = _repo.BuscarPorNombre(actual.NombreUsuario);
        if (enBd == null || enBd.Id != actual.Id) throw new ArgumentException("El usuario ya no existe.");
        if (!_hasheador.Verificar(claveActual, enBd.Clave)) throw new ArgumentException("La clave actual es incorrecta.");

        if (!Validaciones.CredencialValida(claveNueva)) throw new ArgumentException(Validaciones.MensajeCredencial);
        if (claveNueva.Length < 4) throw new ArgumentException("La clave nueva debe tener al menos 4 caracteres.");
        if (claveNueva == claveActual) throw new ArgumentException("La clave nueva debe ser distinta de la actual.");
        if (claveNueva != confirmacion) throw new ArgumentException("La confirmación no coincide con la clave nueva.");

        // Se guarda el hash de la clave nueva, nunca la clave escrita
        var hash = _hasheador.Hashear(claveNueva);
        _repo.ActualizarClave(actual.Id, hash);
        actual.Clave = hash;   // mantiene la sesión al día
    }
}
