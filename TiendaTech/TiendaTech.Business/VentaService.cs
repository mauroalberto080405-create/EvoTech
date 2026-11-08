using TiendaTech.Data;
using TiendaTech.Models;

namespace TiendaTech.Business;

public interface IVentaService
{
    decimal CalcularDescuento(decimal subtotal);
    Venta Registrar(Cliente? cliente, List<DetalleVenta> detalles);
    List<VentaResumen> ListarHistorial();
    List<DetalleVenta> ObtenerDetalles(int ventaId);
    void Anular(int ventaId);
}

public class VentaService : IVentaService
{
    private readonly IVentaRepository _repo;
    private readonly IDescuento _descuento;
    private readonly ISesion _sesion;

    // DIP: recibe abstracciones por constructor; no crea "new VentaSqlRepository()" ni "new DescuentoPorMonto()"
    public VentaService(IVentaRepository repo, IDescuento descuento, ISesion sesion)
    {
        _repo = repo;
        _descuento = descuento;
        _sesion = sesion;
    }

    public decimal CalcularDescuento(decimal subtotal) => _descuento.Calcular(subtotal);
    public List<VentaResumen> ListarHistorial() => _repo.ListarResumen();
    public List<DetalleVenta> ObtenerDetalles(int ventaId) => _repo.ObtenerDetalles(ventaId);

    // Anula la venta: devuelve el stock y la deja marcada como anulada (no se borra)
    public void Anular(int ventaId)
    {
        if (ventaId <= 0) throw new ArgumentException("Seleccione una venta.");
        if (_sesion.UsuarioActual == null) throw new ArgumentException("No hay un usuario con sesión iniciada.");

        _repo.Anular(ventaId); // el repositorio valida que exista y que no esté anulada
    }

    public Venta Registrar(Cliente? cliente, List<DetalleVenta> detalles)
    {
        if (cliente == null) throw new ArgumentException("Seleccione un cliente.");
        if (detalles.Count == 0) throw new ArgumentException("Agregue al menos un producto.");
        if (_sesion.UsuarioActual == null) throw new ArgumentException("No hay un usuario con sesión iniciada.");

        foreach (var d in detalles)
            if (d.Cantidad > d.Producto.Stock)
                throw new ArgumentException($"Stock insuficiente de {d.Producto.Nombre}.");

        var subtotal = detalles.Sum(d => d.Subtotal);
        var venta = new Venta
        {
            Cliente = cliente,
            Vendedor = _sesion.UsuarioActual,
            Detalles = detalles.ToList(),
            Subtotal = subtotal,
            Descuento = _descuento.Calcular(subtotal)
        };

        _repo.Agregar(venta); // guarda venta + detalle y descuenta stock
        return venta;
    }
}
