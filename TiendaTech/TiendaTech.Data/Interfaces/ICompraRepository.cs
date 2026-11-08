using TiendaTech.Models;

namespace TiendaTech.Data;

public interface ICompraRepository
{
    // Guarda la compra, sus detalles y AUMENTA el stock (todo o nada)
    void Agregar(Compra compra);
    // true si ese proveedor ya tiene registrado ese N° de factura/guía (evita ingresar dos veces la misma entrega)
    bool ExisteDocumento(int proveedorId, string numeroDocumento);
    List<CompraResumen> ListarResumen();
    List<DetalleCompra> ObtenerDetalles(int compraId);
}
