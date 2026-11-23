namespace TiendaTech.Models;

// Una línea del kardex de un producto: entrada (compra), salida (venta) o anulación (devuelve stock)
public class MovimientoKardex
{
    public DateTime? Fecha { get; set; }       // null en la fila "Saldo inicial"
    public string Tipo { get; set; } = "";     // Saldo inicial / Entrada (compra) / Salida (venta) / Anulación de venta
    public string Documento { get; set; } = "";
    public int Entrada { get; set; }
    public int Salida { get; set; }
    public int Saldo { get; set; }             // stock del producto después de este movimiento
}

// Resultado completo del kardex de un producto
public class ResumenKardex
{
    public Producto Producto { get; set; } = new();
    public List<MovimientoKardex> Movimientos { get; set; } = new();
    public int TotalEntradas { get; set; }
    public int TotalSalidas { get; set; }
}
