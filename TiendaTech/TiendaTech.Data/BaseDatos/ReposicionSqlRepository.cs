using TiendaTech.Models;

namespace TiendaTech.Data;

// Solo lee datos: no modifica productos ni stock
public class ReposicionSqlRepository : IReposicionRepository
{
    private readonly IConexionFactory _fabrica;
    public ReposicionSqlRepository(IConexionFactory fabrica)
    {
        _fabrica = fabrica;
        EsquemaProductos.Asegurar(fabrica); // necesita la columna StockMinimo
    }

    public List<LineaReposicion> ProductosPorReponer()
    {
        using var cn = _fabrica.Abrir();
        using var cmd = cn.Comando("""
            SELECT p.Id, p.Nombre, p.Marca, p.Stock, p.StockMinimo, p.Costo, p.ProveedorId,
                   COALESCE(pr.Empresa, 'Sin proveedor') AS Proveedor,
                   COALESCE(pr.Ruc, '') AS Ruc,
                   COALESCE(pr.Telefono, '') AS Telefono
            FROM Producto p
            LEFT JOIN Proveedor pr ON pr.Id = p.ProveedorId
            WHERE p.Stock <= p.StockMinimo
            ORDER BY (p.ProveedorId IS NULL), Proveedor, p.Nombre;
            """);
        using var r = cmd.ExecuteReader();
        var lista = new List<LineaReposicion>();
        while (r.Read())
            lista.Add(new LineaReposicion
            {
                ProductoId = r.Entero("Id"),
                Producto = r.Texto("Nombre"),
                Marca = r.Texto("Marca"),
                Stock = r.Entero("Stock"),
                StockMinimo = r.Entero("StockMinimo"),
                CostoUnitario = r.Decimal("Costo"),
                ProveedorId = r.EnteroNulo("ProveedorId"),
                Proveedor = r.Texto("Proveedor"),
                RucProveedor = r.Texto("Ruc"),
                TelefonoProveedor = r.Texto("Telefono")
            });
        return lista;
    }
}
