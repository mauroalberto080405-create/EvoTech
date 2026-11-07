using TiendaTech.Models;

namespace TiendaTech.Data;

public interface ICompraRepository
{
    // Guarda la compra, sus detalles y AUMENTA el stock (todo o nada)
    void Agregar(Compra compra);
    List<CompraResumen> ListarResumen();
    List<DetalleCompra> ObtenerDetalles(int compraId);
}
