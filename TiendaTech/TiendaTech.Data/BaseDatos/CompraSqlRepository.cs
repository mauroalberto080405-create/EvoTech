using TiendaTech.Models;

namespace TiendaTech.Data;

public class CompraSqlRepository : ICompraRepository
{
    private readonly IConexionFactory _fabrica;
    public CompraSqlRepository(IConexionFactory fabrica)
    {
        _fabrica = fabrica;
        EsquemaCompras.Asegurar(fabrica); // columna NumeroDocumento (actualiza BD antiguas sin borrar datos)
    }

    public bool ExisteDocumento(int proveedorId, string numeroDocumento)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("""
            SELECT COUNT(*) FROM Compra
            WHERE ProveedorId = @proveedor AND NumeroDocumento = @documento;
            """).Param("@proveedor", proveedorId).Param("@documento", numeroDocumento);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    // Transaccion: compra + detalles + aumento de stock
    public void Agregar(Compra compra)
    {
        using var cn = _fabrica.Abrir();
        using var tx = cn.BeginTransaction();
        try
        {
            using (var cmd = cn.Comando("""
                INSERT INTO Compra (Fecha, ProveedorId, UsuarioId, Total, NumeroDocumento)
                VALUES (@fecha, @proveedor, @usuario, @total, @documento);
                """, tx))
            {
                cmd.Param("@fecha", compra.Fecha.ToString(DbExtensiones.FormatoFecha))
                   .Param("@proveedor", compra.Proveedor!.Id).Param("@usuario", compra.Usuario!.Id)
                   .Param("@total", compra.Total).Param("@documento", compra.NumeroDocumento);
                cmd.ExecuteNonQuery();
            }
            compra.Id = cn.UltimoId(tx);

            foreach (var d in compra.Detalles)
            {
                using (var cmd = cn.Comando("""
                    INSERT INTO DetalleCompra (CompraId, ProductoId, Cantidad, CostoUnitario, Subtotal)
                    VALUES (@compra, @producto, @cantidad, @costo, @subtotal);
                    """, tx))
                {
                    cmd.Param("@compra", compra.Id).Param("@producto", d.Producto.Id)
                       .Param("@cantidad", d.Cantidad).Param("@costo", d.CostoUnitario)
                       .Param("@subtotal", d.Subtotal);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = cn.Comando("UPDATE Producto SET Stock = Stock + @cantidad WHERE Id = @id;", tx))
                {
                    cmd.Param("@cantidad", d.Cantidad).Param("@id", d.Producto.Id);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException($"El producto {d.Producto.Nombre} no existe.");
                }
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            compra.Id = 0;
            throw;
        }

        foreach (var d in compra.Detalles) d.Producto.Stock += d.Cantidad;
    }

    public List<CompraResumen> ListarResumen()
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("""
            SELECT c.Id, c.Fecha, p.Empresa AS Proveedor, c.NumeroDocumento, u.NombreUsuario AS Usuario,
                   COALESCE(SUM(d.Cantidad), 0) AS Unidades, c.Total
            FROM Compra c
            INNER JOIN Proveedor p ON p.Id = c.ProveedorId
            INNER JOIN Usuario u ON u.Id = c.UsuarioId
            LEFT JOIN DetalleCompra d ON d.CompraId = c.Id
            GROUP BY c.Id, c.Fecha, p.Empresa, c.NumeroDocumento, u.NombreUsuario, c.Total
            ORDER BY c.Id DESC;
            """);
        using var r = cmd.ExecuteReader();
        var lista = new List<CompraResumen>();
        while (r.Read())
            lista.Add(new CompraResumen
            {
                Id = r.Entero("Id"),
                Fecha = r.Fecha("Fecha"),
                Proveedor = r.Texto("Proveedor"),
                NumeroDocumento = r.Texto("NumeroDocumento"), // "" en compras anteriores a esta mejora
                Usuario = r.Texto("Usuario"),
                Unidades = r.Entero("Unidades"),
                Total = r.Decimal("Total")
            });
        return lista;
    }

    public List<DetalleCompra> ObtenerDetalles(int compraId)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("""
            SELECT d.ProductoId, p.Nombre, p.Marca, d.Cantidad, d.CostoUnitario
            FROM DetalleCompra d
            INNER JOIN Producto p ON p.Id = d.ProductoId
            WHERE d.CompraId = @compra
            ORDER BY d.Id;
            """).Param("@compra", compraId);
        using var r = cmd.ExecuteReader();
        var lista = new List<DetalleCompra>();
        while (r.Read())
            lista.Add(new DetalleCompra
            {
                Producto = new Producto { Id = r.Entero("ProductoId"), Nombre = r.Texto("Nombre"), Marca = r.Texto("Marca") },
                Cantidad = r.Entero("Cantidad"),
                CostoUnitario = r.Decimal("CostoUnitario")
            });
        return lista;
    }
}
