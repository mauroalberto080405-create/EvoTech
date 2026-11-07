using TiendaTech.Data;
using TiendaTech.Models;

namespace TiendaTech.Business;

public interface ICompraService
{
    Compra Registrar(Proveedor? proveedor, List<DetalleCompra> detalles);
    List<CompraResumen> ListarHistorial();
    List<DetalleCompra> ObtenerDetalles(int compraId);
}

public class CompraService : ICompraService
{
    private readonly ICompraRepository _repo;
    private readonly IProductoRepository _productos;
    private readonly ISesion _sesion;

    public CompraService(ICompraRepository repo, IProductoRepository productos, ISesion sesion)
    {
        _repo = repo;
        _productos = productos;
        _sesion = sesion;
    }

    public List<CompraResumen> ListarHistorial() => _repo.ListarResumen();
    public List<DetalleCompra> ObtenerDetalles(int compraId) => _repo.ObtenerDetalles(compraId);

    public Compra Registrar(Proveedor? proveedor, List<DetalleCompra> detalles)
    {
        if (proveedor == null) throw new ArgumentException("Seleccione un proveedor.");
        if (detalles.Count == 0) throw new ArgumentException("Agregue al menos un producto.");
        if (_sesion.UsuarioActual == null) throw new ArgumentException("No hay un usuario con sesión iniciada.");
        foreach (var d in detalles)
        {
            if (d.Cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a 0.");

            // El costo SIEMPRE se toma de la base de datos: se ignora cualquier valor que mande la pantalla
            var real = _productos.ObtenerPorId(d.Producto.Id)
                       ?? throw new ArgumentException($"El producto {d.Producto.Nombre} ya no existe.");
            if (real.Costo <= 0)
                throw new ArgumentException($"El producto {real.Nombre} no tiene un costo válido registrado. Regístrelo en la pantalla Productos (botón Actualizar).");
            d.CostoUnitario = real.Costo;
        }

        var compra = new Compra { Proveedor = proveedor, Usuario = _sesion.UsuarioActual, Detalles = detalles.ToList() };
        _repo.Agregar(compra); // guarda y aumenta el stock
        return compra;
    }
}
