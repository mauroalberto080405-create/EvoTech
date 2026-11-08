namespace TiendaTech.Models;

public class Producto : IEntidad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Marca { get; set; } = "";
    public decimal Precio { get; set; }
    public decimal Costo { get; set; }   // costo unitario de compra (se usa en Aumentar stock)
    public int Stock { get; set; }
    public int StockMinimo { get; set; } = 5;   // la alerta de stock bajo usa el mínimo de cada producto
    public bool EsStockBajo => Stock <= StockMinimo;
    public int? ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = "";
    public override string ToString() => $"{Nombre} ({Marca}) - S/ {Precio:0.00} - Stock: {Stock}";
}
