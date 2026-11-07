using TiendaTech.Models;

namespace TiendaTech.Business;

// Guarda quien inicio sesion (para registrar que vendedor hizo cada venta)
public interface ISesion
{
    Usuario? UsuarioActual { get; }
    void Iniciar(Usuario usuario);
    void Cerrar();
}

public class Sesion : ISesion
{
    public Usuario? UsuarioActual { get; private set; }
    public void Iniciar(Usuario usuario) => UsuarioActual = usuario;
    public void Cerrar() => UsuarioActual = null;
}
