namespace TiendaTech.Models;

public class ProductoVendido
{
    public string Producto { get; set; } = "";
    public int Unidades { get; set; }
    public decimal Importe { get; set; }
}

public class VentasPorVendedor
{
    public string Vendedor { get; set; } = "";
    public int Ventas { get; set; }
    public decimal Total { get; set; }
}
