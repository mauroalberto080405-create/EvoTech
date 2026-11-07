using TiendaTech.Models;

namespace TiendaTech.Data;

public interface IProductoRepository : IRepository<Producto>
{
    bool TieneMovimientos(int id); // ventas o compras
    Producto? ObtenerPorId(int id);
    void Actualizar(Producto producto);
}
