namespace TiendaTech.Models;

public class DetalleVenta
{
    public Producto Producto { get; set; } = new();
    public int Cantidad { get; set; }
    public string NombreProducto => Producto.Nombre;
    public decimal PrecioUnitario => Producto.Precio;
    public decimal Subtotal => Producto.Precio * Cantidad;
}
