using TiendaTech.Models;

namespace TiendaTech.Data;

// Movimientos "en bruto" de un producto (sin saldo): compras, ventas y anulaciones, en orden cronológico
public interface IKardexRepository
{
    List<MovimientoKardex> ObtenerMovimientos(int productoId);
}
