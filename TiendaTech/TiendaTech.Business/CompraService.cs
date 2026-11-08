using System.Text.RegularExpressions;
using TiendaTech.Data;
using TiendaTech.Models;

namespace TiendaTech.Business;

public interface ICompraService
{
    // Productos que entrega ESE proveedor (los únicos que se pueden ingresar con su factura/guía)
    List<Producto> ProductosDelProveedor(Proveedor? proveedor);
    Compra Registrar(Proveedor? proveedor, string numeroDocumento, List<DetalleCompra> detalles);
    List<CompraResumen> ListarHistorial();
    List<DetalleCompra> ObtenerDetalles(int compraId);
}

public class CompraService : ICompraService
{
    // Serie de 4 caracteres + guion + número: F001-000123 (factura), T001-45 (guía), EG01-7 (guía electrónica)
    private static readonly Regex FormatoDocumento = new(@"^[A-Z0-9]{4}-\d{1,8}$");
    public const string MensajeFormatoDocumento =
        "Ingrese el N° de factura o guía del proveedor con el formato SERIE-NÚMERO, por ejemplo F001-000123.";

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

    public List<Producto> ProductosDelProveedor(Proveedor? proveedor) =>
        proveedor == null
            ? new List<Producto>()
            : _productos.ObtenerTodos().Where(p => p.ProveedorId == proveedor.Id).OrderBy(p => p.Nombre).ToList();

    // Deja el N° en mayúsculas y sin espacios: " f001-000123 " -> "F001-000123"
    public static string NormalizarDocumento(string? numero) => (numero ?? "").Trim().ToUpperInvariant();

    public Compra Registrar(Proveedor? proveedor, string numeroDocumento, List<DetalleCompra> detalles)
    {
        if (proveedor == null) throw new ArgumentException("Seleccione un proveedor.");

        var documento = NormalizarDocumento(numeroDocumento);
        if (!FormatoDocumento.IsMatch(documento)) throw new ArgumentException(MensajeFormatoDocumento);
        if (_repo.ExisteDocumento(proveedor.Id, documento))
            throw new ArgumentException($"La factura/guía {documento} de {proveedor.Empresa} ya fue registrada. " +
                                        "Revise el Historial de compras: no se puede ingresar dos veces la misma entrega.");

        if (detalles.Count == 0) throw new ArgumentException("Agregue al menos un producto.");
        if (_sesion.UsuarioActual == null) throw new ArgumentException("No hay un usuario con sesión iniciada.");
        foreach (var d in detalles)
        {
            if (d.Cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a 0.");

            // El costo SIEMPRE se toma de la base de datos: se ignora cualquier valor que mande la pantalla
            var real = _productos.ObtenerPorId(d.Producto.Id)
                       ?? throw new ArgumentException($"El producto {d.Producto.Nombre} ya no existe.");

            // Solo se ingresan productos que entrega este proveedor (la regla vale aunque se salte la pantalla)
            if (real.ProveedorId != proveedor.Id)
                throw new ArgumentException(
                    $"El producto {real.Nombre} no lo entrega {proveedor.Empresa}" +
                    (string.IsNullOrEmpty(real.ProveedorNombre) ? "." : $" (su proveedor es {real.ProveedorNombre}).") +
                    " Solo se pueden ingresar productos del proveedor de la factura/guía.");

            if (real.Costo <= 0)
                throw new ArgumentException($"El producto {real.Nombre} no tiene un costo válido registrado. Regístrelo en la pantalla Productos (botón Actualizar).");
            d.CostoUnitario = real.Costo;
        }

        var compra = new Compra
        {
            Proveedor = proveedor,
            NumeroDocumento = documento,
            Usuario = _sesion.UsuarioActual,
            Detalles = detalles.ToList()
        };
        _repo.Agregar(compra); // guarda y aumenta el stock
        return compra;
    }
}
