using TiendaTech.Data;
using TiendaTech.Models;

namespace TiendaTech.Business;

public interface IKardexService
{
    ResumenKardex Consultar(int productoId);
}

public class KardexService : IKardexService
{
    private readonly IKardexRepository _kardex;
    private readonly IProductoRepository _productos;
    public KardexService(IKardexRepository kardex, IProductoRepository productos)
    {
        _kardex = kardex;
        _productos = productos;
    }

    // El saldo final siempre coincide con el stock actual del producto: si hubo stock
    // anterior al primer movimiento registrado, se muestra como fila "Saldo inicial".
    public ResumenKardex Consultar(int productoId)
    {
        var producto = _productos.ObtenerPorId(productoId)
            ?? throw new ArgumentException("El producto ya no existe.");
        var movimientos = _kardex.ObtenerMovimientos(productoId);

        int entradas = movimientos.Sum(m => m.Entrada);
        int salidas = movimientos.Sum(m => m.Salida);
        int saldo = producto.Stock - entradas + salidas; // stock antes del primer movimiento

        var filas = new List<MovimientoKardex>();
        if (saldo != 0)
            filas.Add(new MovimientoKardex { Tipo = "Saldo inicial", Documento = "Stock previo a los movimientos", Saldo = saldo });

        foreach (var m in movimientos)
        {
            saldo += m.Entrada - m.Salida;
            m.Saldo = saldo;
            filas.Add(m);
        }

        return new ResumenKardex { Producto = producto, Movimientos = filas, TotalEntradas = entradas, TotalSalidas = salidas };
    }
}
