using TiendaTech.Models;

namespace TiendaTech.Data;

public interface IProveedorRepository : IRepository<Proveedor>
{
    bool ExisteRuc(string ruc);
    Proveedor? ObtenerPorId(int id);
    void Actualizar(Proveedor proveedor); // cambia empresa y teléfono (el RUC identifica al proveedor y no se edita)
    bool TieneMovimientos(int id); // productos o compras
}
