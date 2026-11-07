using TiendaTech.Models;

namespace TiendaTech.Data;

public interface IProveedorRepository : IRepository<Proveedor>
{
    bool ExisteRuc(string ruc);
    bool TieneMovimientos(int id); // productos o compras
}
