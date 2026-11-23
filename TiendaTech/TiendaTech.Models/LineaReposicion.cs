namespace TiendaTech.Models;

// Una fila del pedido de reposición: un producto en o bajo su stock mínimo y cuánto pedir al proveedor
public class LineaReposicion
{
    public int ProductoId { get; set; }
    public string Producto { get; set; } = "";
    public string Marca { get; set; } = "";
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public decimal CostoUnitario { get; set; }      // costo de compra guardado en el producto

    public int? ProveedorId { get; set; }            // null = el producto no tiene proveedor asignado
    public string Proveedor { get; set; } = "";
    public string RucProveedor { get; set; } = "";
    public string TelefonoProveedor { get; set; } = "";

    public int CantidadSugerida { get; set; }        // la calcula ReposicionService
    public decimal CostoEstimado => CantidadSugerida * CostoUnitario;
}
