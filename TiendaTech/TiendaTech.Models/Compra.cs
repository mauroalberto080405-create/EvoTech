namespace TiendaTech.Models;

// Compra de mercaderia a un proveedor (aumenta el stock)
public class Compra : IEntidad
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public Proveedor? Proveedor { get; set; }
    public Usuario? Usuario { get; set; }
    public List<DetalleCompra> Detalles { get; set; } = new();
    public decimal Total => Detalles.Sum(d => d.Subtotal);
}

public class DetalleCompra
{
    public Producto Producto { get; set; } = new();
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public string NombreProducto => Producto.Nombre;
    public decimal Subtotal => CostoUnitario * Cantidad;
}

public class CompraResumen
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Proveedor { get; set; } = "";
    public string Usuario { get; set; } = "";
    public int Unidades { get; set; }
    public decimal Total { get; set; }
}
