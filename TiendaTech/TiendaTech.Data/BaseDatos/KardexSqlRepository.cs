using TiendaTech.Models;

namespace TiendaTech.Data;

// Lee DetalleCompra (entradas), DetalleVenta (salidas) y Venta.FechaAnulacion (devoluciones por anulación)
public class KardexSqlRepository : IKardexRepository
{
    private readonly IConexionFactory _fabrica;
    public KardexSqlRepository(IConexionFactory fabrica)
    {
        _fabrica = fabrica;
        EsquemaVentas.Asegurar(fabrica);  // columnas Anulada y FechaAnulacion
        EsquemaCompras.Asegurar(fabrica); // columna NumeroDocumento
    }

    public List<MovimientoKardex> ObtenerMovimientos(int productoId)
    {
        using var cn = _fabrica.Abrir();
        // Orden: fecha, y a igual fecha primero compras, luego ventas, luego anulaciones
        using var cmd = cn.Comando("""
            SELECT c.Fecha AS Fecha, 0 AS Orden, c.Id AS DocId, COALESCE(c.NumeroDocumento, '') AS Doc,
                   SUM(d.Cantidad) AS Cantidad
            FROM DetalleCompra d
            INNER JOIN Compra c ON c.Id = d.CompraId
            WHERE d.ProductoId = @id
            GROUP BY c.Id, c.Fecha, c.NumeroDocumento
            UNION ALL
            SELECT v.Fecha, 1, v.Id, '', SUM(d.Cantidad)
            FROM DetalleVenta d
            INNER JOIN Venta v ON v.Id = d.VentaId
            WHERE d.ProductoId = @id
            GROUP BY v.Id, v.Fecha
            UNION ALL
            SELECT COALESCE(v.FechaAnulacion, v.Fecha), 2, v.Id, '', SUM(d.Cantidad)
            FROM DetalleVenta d
            INNER JOIN Venta v ON v.Id = d.VentaId
            WHERE d.ProductoId = @id AND v.Anulada = 1
            GROUP BY v.Id, v.Fecha, v.FechaAnulacion
            ORDER BY Fecha, Orden, DocId;
            """).Param("@id", productoId);

        using var r = cmd.ExecuteReader();
        var lista = new List<MovimientoKardex>();
        while (r.Read())
        {
            int cantidad = r.Entero("Cantidad");
            var m = new MovimientoKardex { Fecha = r.Fecha("Fecha") };
            switch (r.Entero("Orden"))
            {
                case 0:
                    var factura = r.Texto("Doc");
                    m.Tipo = "Entrada (compra)";
                    m.Documento = factura.Length > 0 ? $"Compra N° {r.Entero("DocId")} · {factura}" : $"Compra N° {r.Entero("DocId")}";
                    m.Entrada = cantidad;
                    break;
                case 1:
                    m.Tipo = "Salida (venta)";
                    m.Documento = $"Venta N° {r.Entero("DocId")}";
                    m.Salida = cantidad;
                    break;
                default:
                    m.Tipo = "Anulación de venta";
                    m.Documento = $"Venta N° {r.Entero("DocId")} anulada";
                    m.Entrada = cantidad;
                    break;
            }
            lista.Add(m);
        }
        return lista;
    }
}
