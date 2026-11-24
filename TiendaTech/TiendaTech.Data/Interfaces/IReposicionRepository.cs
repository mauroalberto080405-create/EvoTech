using TiendaTech.Models;

namespace TiendaTech.Data;

public interface IReposicionRepository
{
    // Productos con Stock <= StockMinimo, con los datos de su proveedor (ordenados por proveedor y producto)
    List<LineaReposicion> ProductosPorReponer();
}
