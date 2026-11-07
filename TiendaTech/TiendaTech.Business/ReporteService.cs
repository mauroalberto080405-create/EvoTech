using TiendaTech.Data;
using TiendaTech.Models;

namespace TiendaTech.Business;

public interface IReporteService
{
    List<ProductoVendido> ProductosMasVendidos(DateTime desde, DateTime hasta);
    List<VentasPorVendedor> VentasPorVendedor(DateTime desde, DateTime hasta);
    void ExportarExcel(DateTime desde, DateTime hasta, string rutaArchivo);
}

public class ReporteService : IReporteService
{
    private readonly IReporteRepository _repo;
    public ReporteService(IReporteRepository repo) => _repo = repo;

    public List<ProductoVendido> ProductosMasVendidos(DateTime desde, DateTime hasta)
    {
        ValidarRango(desde, hasta);
        return _repo.ProductosMasVendidos(desde, hasta);
    }

    public List<VentasPorVendedor> VentasPorVendedor(DateTime desde, DateTime hasta)
    {
        ValidarRango(desde, hasta);
        return _repo.VentasPorVendedor(desde, hasta);
    }

    // Genera un .xlsx real con 4 hojas: Resumen, Ventas, Productos y Vendedores
    public void ExportarExcel(DateTime desde, DateTime hasta, string rutaArchivo)
    {
        ValidarRango(desde, hasta);
        if (string.IsNullOrWhiteSpace(rutaArchivo)) throw new ArgumentException("Elija dónde guardar el archivo.");

        var ventas = _repo.VentasEnRango(desde, hasta);
        if (ventas.Count == 0)
            throw new ArgumentException("No hay ventas entre las fechas seleccionadas. No se generó ningún archivo.");
        var productos = _repo.ProductosMasVendidos(desde, hasta);
        var vendedores = _repo.VentasPorVendedor(desde, hasta);

        var resumen = new HojaExcel("Resumen", new[] { "Dato", "Valor" }, new[] { 28.0, 22.0 });
        resumen.Filas.Add(new object[] { "Reporte de ventas", "EvoTech" });
        resumen.Filas.Add(new object[] { "Desde", desde.Date });
        resumen.Filas.Add(new object[] { "Hasta", hasta.Date });
        resumen.Filas.Add(new object[] { "Generado", DateTime.Now });
        resumen.Filas.Add(new object[] { "Ventas en el periodo", ventas.Count });
        resumen.Filas.Add(new object[] { "Unidades vendidas", ventas.Sum(v => v.Unidades) });
        resumen.Filas.Add(new object[] { "Subtotal (S/)", ventas.Sum(v => v.Subtotal) });
        resumen.Filas.Add(new object[] { "Descuentos (S/)", ventas.Sum(v => v.Descuento) });
        resumen.Filas.Add(new object[] { "Total vendido (S/)", ventas.Sum(v => v.Total) });

        var hojaVentas = new HojaExcel("Ventas",
            new[] { "N° venta", "Fecha", "Cliente", "Vendedor", "Unidades", "Subtotal (S/)", "Descuento (S/)", "Total (S/)" },
            new[] { 10.0, 18, 26, 14, 10, 14, 14, 14 });
        foreach (var v in ventas)
            hojaVentas.Filas.Add(new object[] { v.Id, v.Fecha, v.Cliente, v.Vendedor, v.Unidades, v.Subtotal, v.Descuento, v.Total });

        var hojaProductos = new HojaExcel("Productos mas vendidos",
            new[] { "Producto", "Unidades", "Importe (S/)" }, new[] { 32.0, 12, 16 });
        foreach (var p in productos) hojaProductos.Filas.Add(new object[] { p.Producto, p.Unidades, p.Importe });

        var hojaVendedores = new HojaExcel("Ventas por vendedor",
            new[] { "Vendedor", "N° ventas", "Total (S/)" }, new[] { 18.0, 12, 16 });
        foreach (var v in vendedores) hojaVendedores.Filas.Add(new object[] { v.Vendedor, v.Ventas, v.Total });

        ExportadorExcel.Guardar(rutaArchivo, new[] { resumen, hojaVentas, hojaProductos, hojaVendedores });
    }

    private static void ValidarRango(DateTime desde, DateTime hasta)
    {
        if (desde.Date > hasta.Date) throw new ArgumentException("La fecha 'Desde' no puede ser mayor que 'Hasta'.");
    }
}
