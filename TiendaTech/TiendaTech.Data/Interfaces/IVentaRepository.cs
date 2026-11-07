using TiendaTech.Models;

namespace TiendaTech.Data;

public interface IVentaRepository
{
    // Guarda la venta, sus detalles y descuenta el stock (todo o nada)
    void Agregar(Venta venta);
    List<VentaResumen> ListarResumen();
    List<DetalleVenta> ObtenerDetalles(int ventaId);
}
