using TiendaTech.Models;

namespace TiendaTech.Data;

public interface IVentaRepository
{
    // Guarda la venta, sus detalles y descuenta el stock (todo o nada)
    void Agregar(Venta venta);
    List<VentaResumen> ListarResumen();
    List<DetalleVenta> ObtenerDetalles(int ventaId);

    // Marca la venta como anulada y devuelve el stock de sus productos (todo o nada)
    void Anular(int ventaId);
}
