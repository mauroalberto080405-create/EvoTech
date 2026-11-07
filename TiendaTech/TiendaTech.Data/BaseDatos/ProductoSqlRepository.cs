using TiendaTech.Models;

namespace TiendaTech.Data;

public class ProductoSqlRepository : IProductoRepository
{
    private readonly IConexionFactory _fabrica;
    public ProductoSqlRepository(IConexionFactory fabrica) => _fabrica = fabrica;

    public List<Producto> ObtenerTodos()
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("""
            SELECT p.Id, p.Nombre, p.Marca, p.Precio, p.Costo, p.Stock, p.ProveedorId,
                   COALESCE(pr.Empresa, '') AS ProveedorNombre
            FROM Producto p
            LEFT JOIN Proveedor pr ON pr.Id = p.ProveedorId
            ORDER BY p.Id;
            """);
        using var r = cmd.ExecuteReader();
        var lista = new List<Producto>();
        while (r.Read()) lista.Add(Leer(r));
        return lista;
    }

    public Producto? ObtenerPorId(int id)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("""
            SELECT p.Id, p.Nombre, p.Marca, p.Precio, p.Costo, p.Stock, p.ProveedorId,
                   COALESCE(pr.Empresa, '') AS ProveedorNombre
            FROM Producto p
            LEFT JOIN Proveedor pr ON pr.Id = p.ProveedorId
            WHERE p.Id = @id;
            """).Param("@id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Leer(r) : null;
    }

    private static Producto Leer(System.Data.Common.DbDataReader r) => new()
    {
        Id = r.Entero("Id"),
        Nombre = r.Texto("Nombre"),
        Marca = r.Texto("Marca"),
        Precio = r.Decimal("Precio"),
        Costo = r.Decimal("Costo"),
        Stock = r.Entero("Stock"),
        ProveedorId = r.EnteroNulo("ProveedorId"),
        ProveedorNombre = r.Texto("ProveedorNombre")
    };

    // Edita los datos del producto. El stock NO se toca aquí: solo cambia con ventas y "Aumentar stock".
    public void Actualizar(Producto p)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("""
            UPDATE Producto
            SET Nombre = @nombre, Marca = @marca, Precio = @precio, Costo = @costo, ProveedorId = @prov
            WHERE Id = @id;
            """)
            .Param("@nombre", p.Nombre).Param("@marca", p.Marca).Param("@precio", p.Precio)
            .Param("@costo", p.Costo).Param("@prov", p.ProveedorId).Param("@id", p.Id);
        if (cmd.ExecuteNonQuery() == 0) throw new InvalidOperationException("El producto ya no existe.");
    }

    public void Agregar(Producto p)
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("""
            INSERT INTO Producto (Nombre, Marca, Precio, Costo, Stock, ProveedorId)
            VALUES (@nombre, @marca, @precio, @costo, @stock, @prov);
            """)
            .Param("@nombre", p.Nombre).Param("@marca", p.Marca).Param("@precio", p.Precio).Param("@costo", p.Costo)
            .Param("@stock", p.Stock).Param("@prov", p.ProveedorId);
        cmd.ExecuteNonQuery();
        p.Id = cn.UltimoId();
    }

    public void Eliminar(int id)
    {
        using var cn = _fabrica.Abrir();
        cn.EliminarPorId("Producto", id);
    }

    public bool TieneMovimientos(int id)
    {
        using var cn = _fabrica.Abrir();
        return cn.Contar("""
            SELECT (SELECT COUNT(*) FROM DetalleVenta WHERE ProductoId = @id)
                 + (SELECT COUNT(*) FROM DetalleCompra WHERE ProductoId = @id);
            """, id) > 0;
    }
}
