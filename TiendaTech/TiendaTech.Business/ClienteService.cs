using TiendaTech.Data;
using TiendaTech.Models;

namespace TiendaTech.Business;

public interface IClienteService
{
    List<Cliente> Listar();
    void Registrar(Cliente cliente);
    void Eliminar(Cliente? cliente);
}

// SRP: solo valida reglas de clientes. No sabe de pantallas ni de SQL.
public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repo;
    public ClienteService(IClienteRepository repo) => _repo = repo;

    public List<Cliente> Listar() => _repo.ObtenerTodos();

    public void Registrar(Cliente c)
    {
        if (string.IsNullOrWhiteSpace(c.Nombre)) throw new ArgumentException("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(c.Documento)) throw new ArgumentException("El documento es obligatorio.");
        if (!c.Documento.All(char.IsDigit) || (c.Documento.Length != 8 && c.Documento.Length != 11))
            throw new ArgumentException("El documento debe ser DNI (8 dígitos) o RUC (11 dígitos).");
        if (!Validaciones.TelefonoValido(c.Telefono)) throw new ArgumentException(Validaciones.MensajeTelefono);
        if (_repo.ExisteDocumento(c.Documento)) throw new ArgumentException("Ya existe un cliente con ese documento.");
        _repo.Agregar(c);
    }

    public void Eliminar(Cliente? c)
    {
        if (c == null) throw new ArgumentException("Seleccione un cliente de la tabla.");
        if (c.Documento == "00000000") throw new ArgumentException("El Cliente General no se puede eliminar.");
        if (_repo.TieneVentas(c.Id))
            throw new ArgumentException($"No se puede eliminar a {c.Nombre}: tiene ventas registradas.");
        _repo.Eliminar(c.Id);
    }
}
