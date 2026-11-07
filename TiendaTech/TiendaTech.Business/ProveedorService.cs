using TiendaTech.Data;
using TiendaTech.Models;

namespace TiendaTech.Business;

public interface IProveedorService
{
    List<Proveedor> Listar();
    void Registrar(Proveedor proveedor);
    void Eliminar(Proveedor? proveedor);
}

public class ProveedorService : IProveedorService
{
    private readonly IProveedorRepository _repo;
    public ProveedorService(IProveedorRepository repo) => _repo = repo;

    public List<Proveedor> Listar() => _repo.ObtenerTodos();

    public void Registrar(Proveedor p)
    {
        p.Empresa = (p.Empresa ?? "").Trim();
        if (p.Empresa.Length < 2)
            throw new ArgumentException("El nombre de la empresa es obligatorio y debe tener al menos 2 caracteres.");
        if (!Validaciones.TelefonoValido(p.Telefono)) throw new ArgumentException(Validaciones.MensajeTelefono);
        if (p.Ruc.Length != 11 || !p.Ruc.All(char.IsDigit)) throw new ArgumentException("El RUC debe tener 11 dígitos.");
        if (_repo.ExisteRuc(p.Ruc)) throw new ArgumentException("Ya existe un proveedor con ese RUC.");
        _repo.Agregar(p);
    }

    public void Eliminar(Proveedor? p)
    {
        if (p == null) throw new ArgumentException("Seleccione un proveedor de la tabla.");
        if (_repo.TieneMovimientos(p.Id))
            throw new ArgumentException($"No se puede eliminar {p.Empresa}: tiene productos o compras asociadas.");
        _repo.Eliminar(p.Id);
    }
}
