namespace TiendaTech.Models;

public class DetalleVenta
{
    public Producto Producto { get; set; } = new();
    public int Cantidad { get; set; }
    public string NombreProducto => Producto.Nombre;
    public decimal PrecioUnitario => Producto.Precio;
    public decimal Subtotal => Producto.Precio * Cantidad;
    // Costo del producto al momento de la venta (en ventas guardadas viene de DetalleVenta.CostoUnitario)
    public decimal CostoUnitario => Producto.Costo;
    public decimal Ganancia => (Producto.Precio - Producto.Costo) * Cantidad;
}
