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
    private readonly IHasheadorClave _hasheador;
    private readonly IBitacoraService? _bitacora;

    // Mientras IBitacoraService no esté registrado en Program.cs, el contenedor usa este
    // constructor y el login funciona igual, solo que sin registrar en la bitácora.
    public AuthService(IUsuarioRepository repo, ISesion sesion, IHasheadorClave hasheador)
        : this(repo, sesion, hasheador, null) { }

    public AuthService(IUsuarioRepository repo, ISesion sesion, IHasheadorClave hasheador, IBitacoraService? bitacora)
    {
        _repo = repo;
        _sesion = sesion;
        _hasheador = hasheador;
        _bitacora = bitacora;
    }

    public Usuario? Validar(string usuario, string clave)
    {
        // La misma regla que la pantalla, para que no se pueda saltar desde otro lado
        if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(clave))
            throw new ArgumentException("Ingrese el usuario y la clave.");
        if (!Validaciones.CredencialValida(usuario) || !Validaciones.CredencialValida(clave))
            throw new ArgumentException(Validaciones.MensajeCredencial);

        var u = _repo.BuscarPorNombre(usuario);
        if (u == null)
        {
            RegistrarAcceso(usuario, BitacoraService.UsuarioNoExiste);
            return null;
        }
        if (!_hasheador.Verificar(clave, u.Clave))
        {
            RegistrarAcceso(usuario, BitacoraService.ClaveIncorrecta);
            return null;
        }
        _sesion.Iniciar(u);
        RegistrarAcceso(u.NombreUsuario, BitacoraService.InicioExitoso);
        return u;
    }

    // Bitácora de accesos: se registra el usuario escrito y el evento, NUNCA la clave.
    // Si la bitácora falla, el inicio de sesión no se bloquea.
    private void RegistrarAcceso(string usuario, string evento)
    {
        try { _bitacora?.Registrar(usuario, evento); }
        catch (Exception) { /* la bitácora es un registro auxiliar: no impide entrar al sistema */ }
    }
}
