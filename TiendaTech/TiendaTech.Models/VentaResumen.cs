namespace TiendaTech.Models;

// Fila del historial de ventas (lo que se muestra en la pantalla Historial)
public class VentaResumen
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Cliente { get; set; } = "";
    public string Vendedor { get; set; } = "";
    public int Unidades { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
}
