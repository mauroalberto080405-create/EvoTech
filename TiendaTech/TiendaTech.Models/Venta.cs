namespace TiendaTech.Models;

public class Venta : IEntidad
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public Cliente? Cliente { get; set; }
    public Usuario? Vendedor { get; set; }
    public List<DetalleVenta> Detalles { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public bool Anulada { get; set; }   // true = venta anulada (se conserva el registro, no se borra)
    public decimal Total => Subtotal - Descuento;
}
