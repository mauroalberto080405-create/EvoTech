using TiendaTech.Data;
using TiendaTech.Models;

namespace TiendaTech.Business;

public interface IBitacoraService
{
    void Registrar(string usuario, string evento);
    List<RegistroBitacora> Listar(DateTime desde, DateTime hasta);
}

public class BitacoraService : IBitacoraService
{
    public const string InicioExitoso = "Inicio exitoso";
    public const string ClaveIncorrecta = "Clave incorrecta";
    public const string UsuarioNoExiste = "Usuario no existe";
    private const int MaxUsuario = 50; // lo escrito en el login se recorta para no guardar textos enormes

    private readonly IBitacoraRepository _repo;
    public BitacoraService(IBitacoraRepository repo) => _repo = repo;

    // Solo recibe el usuario y el evento: la clave escrita nunca llega a la bitácora
    public void Registrar(string usuario, string evento)
    {
        usuario = (usuario ?? "").Trim();
        if (usuario.Length > MaxUsuario) usuario = usuario[..MaxUsuario];
        if (string.IsNullOrWhiteSpace(evento)) throw new ArgumentException("El evento es obligatorio.");
        _repo.Agregar(new RegistroBitacora { Fecha = DateTime.Now, Usuario = usuario, Evento = evento });
    }

    public List<RegistroBitacora> Listar(DateTime desde, DateTime hasta)
    {
        if (desde.Date > hasta.Date) throw new ArgumentException("La fecha 'Desde' no puede ser mayor que 'Hasta'.");
        return _repo.ObtenerEnRango(desde, hasta);
    }
}
