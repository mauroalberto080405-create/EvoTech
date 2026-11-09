using TiendaTech.Models;

namespace TiendaTech.Data;

public interface IReporteRepository
{
    List<ProductoVendido> ProductosMasVendidos(DateTime desde, DateTime hasta);
    List<VentasPorVendedor> VentasPorVendedor(DateTime desde, DateTime hasta);
    List<VentaResumen> VentasEnRango(DateTime desde, DateTime hasta);
    DateTime? FechaUltimaVenta();   // null si todavía no hay ventas
}
