using TiendaTech.Models;

namespace TiendaTech.Data;

public class VentaSqlRepository : IVentaRepository
{
    private readonly IConexionFactory _fabrica;
    public VentaSqlRepository(IConexionFactory fabrica)
    {
        _fabrica = fabrica;
        EsquemaVentas.Asegurar(fabrica); // columnas Anulada y FechaAnulacion (actualiza BD antiguas sin borrar datos)
    }

    // Transaccion: se guarda la venta + detalles + descuento de stock, o no se guarda nada
    public void Agregar(Venta venta)
    {
        using var cn = _fabrica.Abrir();
        using var tx = cn.BeginTransaction();
        try
        {
            using (var cmd = cn.Comando("""
                INSERT INTO Venta (Fecha, ClienteId, UsuarioId, Subtotal, Descuento, Total)
                VALUES (@fecha, @cliente, @usuario, @subtotal, @descuento, @total);
                """, tx))
            {
                cmd.Param("@fecha", venta.Fecha.ToString(DbExtensiones.FormatoFecha))
                   .Param("@cliente", venta.Cliente!.Id).Param("@usuario", venta.Vendedor!.Id)
                   .Param("@subtotal", venta.Subtotal).Param("@descuento", venta.Descuento)
                   .Param("@total", venta.Total);
                cmd.ExecuteNonQuery();
            }
            venta.Id = cn.UltimoId(tx);

            foreach (var d in venta.Detalles)
            {
                using (var cmd = cn.Comando("""
                    INSERT INTO DetalleVenta (VentaId, ProductoId, Cantidad, PrecioUnitario, Subtotal)
                    VALUES (@venta, @producto, @cantidad, @precio, @subtotal);
                    """, tx))
                {
                    cmd.Param("@venta", venta.Id).Param("@producto", d.Producto.Id)
                       .Param("@cantidad", d.Cantidad).Param("@precio", d.PrecioUnitario)
                       .Param("@subtotal", d.Subtotal);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = cn.Comando(
                    "UPDATE Producto SET Stock = Stock - @cantidad WHERE Id = @id AND Stock >= @cantidad;", tx))
                {
                    cmd.Param("@cantidad", d.Cantidad).Param("@id", d.Producto.Id);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException($"Stock insuficiente de {d.Producto.Nombre}.");
                }
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            venta.Id = 0;
            throw;
        }

        foreach (var d in venta.Detalles) d.Producto.Stock -= d.Cantidad; // sincroniza los objetos en pantalla
    }

    // Transaccion: se marca la venta como anulada Y se devuelve el stock, o no se hace nada
    public void Anular(int ventaId)
    {
        using var cn = _fabrica.Abrir();
        using var tx = cn.BeginTransaction();
        try
        {
            // Solo se anula si existe y todavia esta activa (evita devolver el stock dos veces)
            using (var cmd = cn.Comando(
                "UPDATE Venta SET Anulada = 1, FechaAnulacion = @fecha WHERE Id = @id AND Anulada = 0;", tx))
            {
                cmd.Param("@fecha", DateTime.Now.ToString(DbExtensiones.FormatoFecha)).Param("@id", ventaId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("La venta no existe o ya fue anulada.");
            }

            // Devuelve al stock lo que se vendio de cada producto
            using (var cmd = cn.Comando("""
                UPDATE Producto
                SET Stock = Stock + (SELECT COALESCE(SUM(d.Cantidad), 0) FROM DetalleVenta d
                                     WHERE d.VentaId = @id AND d.ProductoId = Producto.Id)
                WHERE Id IN (SELECT ProductoId FROM DetalleVenta WHERE VentaId = @id);
                """, tx))
            {
                cmd.Param("@id", ventaId);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public List<VentaResumen> ListarResumen()
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("""
            SELECT v.Id, v.Fecha, c.Nombre AS Cliente, u.NombreUsuario AS Vendedor,
                   COALESCE(SUM(d.Cantidad), 0) AS Unidades,
                   v.Subtotal, v.Descuento, v.Total, v.Anulada
            FROM Venta v
            INNER JOIN Cliente c ON c.Id = v.ClienteId
            INNER JOIN Usuario u ON u.Id = v.UsuarioId
            LEFT JOIN DetalleVenta d ON d.VentaId = v.Id
            GROUP BY v.Id, v.Fecha, c.Nombre, u.NombreUsuario, v.Subtotal, v.Descuento, v.Total, v.Anulada
            ORDER BY v.Id DESC;
            """);
        using var r = cmd.ExecuteReader();
        var lista = new List<VentaResumen>();
        while (r.Read())
            lista.Add(new VentaResumen
            {
                Id = r.Entero("Id"),
                Fecha = r.Fecha("Fecha"),
                Cliente = r.Texto("Cliente"),
                Vendedor = r.Texto("Vendedor"),
                Unidades = r.Entero("Unidades"),
                Subtotal = r.Decimal("Subtotal"),
                Descuento = r.Decimal("Descuento"),
                Total = r.Decimal("Total"),
                Anulada = r.Entero("Anulada") == 1
            });
        return lista;
    }

    public List<DetalleVenta> ObtenerDetalles(int ventaId)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("""
            SELECT d.ProductoId, p.Nombre, p.Marca, d.Cantidad, d.PrecioUnitario
            FROM DetalleVenta d
            INNER JOIN Producto p ON p.Id = d.ProductoId
            WHERE d.VentaId = @venta
            ORDER BY d.Id;
            """).Param("@venta", ventaId);
        using var r = cmd.ExecuteReader();
        var lista = new List<DetalleVenta>();
        while (r.Read())
            lista.Add(new DetalleVenta
            {
                Producto = new Producto
                {
                    Id = r.Entero("ProductoId"),
                    Nombre = r.Texto("Nombre"),
                    Marca = r.Texto("Marca"),
                    Precio = r.Decimal("PrecioUnitario") // precio al momento de la venta
                },
                Cantidad = r.Entero("Cantidad")
            });
        return lista;
    }
}
