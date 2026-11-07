using TiendaTech.Data;
using TiendaTech.Models;

namespace TiendaTech.Business;

public interface IAuthService
{
    Usuario? Validar(string usuario, string clave);
}

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _repo;
    private readonly ISesion _sesion;

    public AuthService(IUsuarioRepository repo, ISesion sesion)
    {
        _repo = repo;
        _sesion = sesion;
    }

    public Usuario? Validar(string usuario, string clave)
    {
        // La misma regla que la pantalla, para que no se pueda saltar desde otro lado
        if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(clave))
            throw new ArgumentException("Ingrese el usuario y la clave.");
        if (!Validaciones.CredencialValida(usuario) || !Validaciones.CredencialValida(clave))
            throw new ArgumentException(Validaciones.MensajeCredencial);

        var u = _repo.BuscarPorNombre(usuario);
        if (u == null || u.Clave != clave) return null;
        _sesion.Iniciar(u);
        return u;
    }
}
