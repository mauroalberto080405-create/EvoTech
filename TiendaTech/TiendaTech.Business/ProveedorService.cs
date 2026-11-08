using TiendaTech.Data;
using TiendaTech.Models;

namespace TiendaTech.Business;

public interface IProveedorService
{
    List<Proveedor> Listar();
    void Registrar(Proveedor proveedor);
    void Actualizar(Proveedor proveedor);
    void Eliminar(Proveedor? proveedor);
}

public class ProveedorService : IProveedorService
{
    // Un RUC peruano empieza con 10 (persona natural), 15, 17 o 20 (empresa)
    private static readonly string[] PrefijosRuc = { "10", "15", "17", "20" };

    private readonly IProveedorRepository _repo;
    public ProveedorService(IProveedorRepository repo) => _repo = repo;

    public List<Proveedor> Listar() => _repo.ObtenerTodos();

    public void Registrar(Proveedor p)
    {
        ValidarDatos(p);
        p.Ruc = (p.Ruc ?? "").Trim();
        if (p.Ruc.Length != 11 || !p.Ruc.All(char.IsDigit)) throw new ArgumentException("El RUC debe tener 11 dígitos.");
        if (!PrefijosRuc.Contains(p.Ruc[..2]))
            throw new ArgumentException("El RUC no es válido: debe empezar con 10, 15, 17 o 20.");
        if (_repo.ExisteRuc(p.Ruc)) throw new ArgumentException("Ya existe un proveedor con ese RUC.");
        _repo.Agregar(p);
    }

    // Edita un proveedor existente: empresa y teléfono. El RUC no cambia (identifica al proveedor).
    public void Actualizar(Proveedor p)
    {
        if (p.Id <= 0) throw new ArgumentException("Seleccione en la tabla el proveedor que desea actualizar.");
        if (_repo.ObtenerPorId(p.Id) == null) throw new ArgumentException("El proveedor seleccionado ya no existe.");
        ValidarDatos(p);
        _repo.Actualizar(p);
    }

    // Reglas comunes a registrar y actualizar
    private static void ValidarDatos(Proveedor p)
    {
        p.Empresa = (p.Empresa ?? "").Trim();
        if (p.Empresa.Length < 2)
            throw new ArgumentException("El nombre de la empresa es obligatorio y debe tener al menos 2 caracteres.");
        if (!Validaciones.TelefonoValido(p.Telefono ?? "")) throw new ArgumentException(Validaciones.MensajeTelefono);
    }

    public void Eliminar(Proveedor? p)
    {
        if (p == null) throw new ArgumentException("Seleccione un proveedor de la tabla.");
        if (_repo.TieneMovimientos(p.Id))
            throw new ArgumentException($"No se puede eliminar {p.Empresa}: tiene productos o compras asociadas.");
        _repo.Eliminar(p.Id);
    }
}
