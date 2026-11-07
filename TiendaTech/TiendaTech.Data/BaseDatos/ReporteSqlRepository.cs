using TiendaTech.Models;

namespace TiendaTech.Data;

public class ReporteSqlRepository : IReporteRepository
{
    private readonly IConexionFactory _fabrica;
    public ReporteSqlRepository(IConexionFactory fabrica) => _fabrica = fabrica;

    public List<ProductoVendido> ProductosMasVendidos(DateTime desde, DateTime hasta)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("""
            SELECT p.Nombre AS Producto, SUM(d.Cantidad) AS Unidades, SUM(d.Subtotal) AS Importe
            FROM DetalleVenta d
            INNER JOIN Venta v ON v.Id = d.VentaId
            INNER JOIN Producto p ON p.Id = d.ProductoId
            WHERE v.Fecha >= @desde AND v.Fecha < @hasta
            GROUP BY p.Id, p.Nombre
            ORDER BY Unidades DESC, Importe DESC;
            """);
        Rango(cmd, desde, hasta);
        using var r = cmd.ExecuteReader();
        var lista = new List<ProductoVendido>();
        while (r.Read())
            lista.Add(new ProductoVendido { Producto = r.Texto("Producto"), Unidades = r.Entero("Unidades"), Importe = r.Decimal("Importe") });
        return lista;
    }

    public List<VentasPorVendedor> VentasPorVendedor(DateTime desde, DateTime hasta)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("""
            SELECT u.NombreUsuario AS Vendedor, COUNT(v.Id) AS Ventas, SUM(v.Total) AS Total
            FROM Venta v
            INNER JOIN Usuario u ON u.Id = v.UsuarioId
            WHERE v.Fecha >= @desde AND v.Fecha < @hasta
            GROUP BY u.Id, u.NombreUsuario
            ORDER BY Total DESC;
            """);
        Rango(cmd, desde, hasta);
        using var r = cmd.ExecuteReader();
        var lista = new List<VentasPorVendedor>();
        while (r.Read())
            lista.Add(new VentasPorVendedor { Vendedor = r.Texto("Vendedor"), Ventas = r.Entero("Ventas"), Total = r.Decimal("Total") });
        return lista;
    }

    public List<VentaResumen> VentasEnRango(DateTime desde, DateTime hasta)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("""
            SELECT v.Id, v.Fecha, c.Nombre AS Cliente, u.NombreUsuario AS Vendedor,
                   COALESCE(SUM(d.Cantidad), 0) AS Unidades, v.Subtotal, v.Descuento, v.Total
            FROM Venta v
            INNER JOIN Cliente c ON c.Id = v.ClienteId
            INNER JOIN Usuario u ON u.Id = v.UsuarioId
            LEFT JOIN DetalleVenta d ON d.VentaId = v.Id
            WHERE v.Fecha >= @desde AND v.Fecha < @hasta
            GROUP BY v.Id, v.Fecha, c.Nombre, u.NombreUsuario, v.Subtotal, v.Descuento, v.Total
            ORDER BY v.Fecha, v.Id;
            """);
        Rango(cmd, desde, hasta);
        using var r = cmd.ExecuteReader();
        var lista = new List<VentaResumen>();
        while (r.Read())
            lista.Add(new VentaResumen
            {
                Id = r.Entero("Id"), Fecha = r.Fecha("Fecha"),
                Cliente = r.Texto("Cliente"), Vendedor = r.Texto("Vendedor"),
                Unidades = r.Entero("Unidades"), Subtotal = r.Decimal("Subtotal"),
                Descuento = r.Decimal("Descuento"), Total = r.Decimal("Total")
            });
        return lista;
    }

    // Las fechas se guardan como texto "yyyy-MM-dd HH:mm:ss", asi que se comparan como texto
    private static void Rango(System.Data.Common.DbCommand cmd, DateTime desde, DateTime hasta) =>
        cmd.Param("@desde", desde.Date.ToString(DbExtensiones.FormatoFecha))
           .Param("@hasta", hasta.Date.AddDays(1).ToString(DbExtensiones.FormatoFecha));
}
