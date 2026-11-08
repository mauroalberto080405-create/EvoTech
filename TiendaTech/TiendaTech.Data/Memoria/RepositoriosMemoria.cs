using TiendaTech.Models;

namespace TiendaTech.Data;

public class ClienteMemoryRepository : MemoryRepository<Cliente>, IClienteRepository
{
    public ClienteMemoryRepository()
    {
        Agregar(new Cliente { Nombre = "Cliente General", Documento = "00000000", Telefono = "-" });
        Agregar(new Cliente { Nombre = "María López", Documento = "45879632", Telefono = "987654321" });
    }

    public bool ExisteDocumento(string documento) => Items.Any(c => c.Documento == documento);
    public bool TieneVentas(int id) => false;
}

public class ProveedorMemoryRepository : MemoryRepository<Proveedor>, IProveedorRepository
{
    public ProveedorMemoryRepository()
    {
        Agregar(new Proveedor { Empresa = "TecnoImport SAC", Ruc = "20123456789", Telefono = "014445555" });
    }

    public bool ExisteRuc(string ruc) => Items.Any(p => p.Ruc == ruc);
    public bool TieneMovimientos(int id) => false;
    public Proveedor? ObtenerPorId(int id) => Items.FirstOrDefault(p => p.Id == id);

    public void Actualizar(Proveedor p)
    {
        var actual = ObtenerPorId(p.Id) ?? throw new InvalidOperationException("El proveedor ya no existe.");
        actual.Empresa = p.Empresa; actual.Telefono = p.Telefono;
    }
}

public class ProductoMemoryRepository : MemoryRepository<Producto>, IProductoRepository
{
    public ProductoMemoryRepository()
    {
        Agregar(new Producto { Nombre = "Celular Galaxy A54", Marca = "Samsung", Precio = 1200m, Costo = 950m, Stock = 10, ProveedorId = 1, ProveedorNombre = "TecnoImport SAC" });
        Agregar(new Producto { Nombre = "Audífonos Tune 520BT", Marca = "JBL", Precio = 150m, Costo = 95m, Stock = 30, ProveedorId = 1, ProveedorNombre = "TecnoImport SAC" });
    }

    public bool TieneMovimientos(int id) => false;
    public Producto? ObtenerPorId(int id) => Items.FirstOrDefault(p => p.Id == id);

    public void Actualizar(Producto p)
    {
        var actual = ObtenerPorId(p.Id) ?? throw new InvalidOperationException("El producto ya no existe.");
        actual.Nombre = p.Nombre; actual.Marca = p.Marca; actual.Precio = p.Precio; actual.Costo = p.Costo;
        actual.ProveedorId = p.ProveedorId; actual.ProveedorNombre = p.ProveedorNombre;
    }
}

public class UsuarioMemoryRepository : IUsuarioRepository
{
    private readonly List<Usuario> _usuarios = new()
    {
        new Usuario { Id = 1, NombreUsuario = "admin", Clave = "1234", Rol = "Administrador" },
        new Usuario { Id = 2, NombreUsuario = "vendedor", Clave = "1234", Rol = "Vendedor" }
    };

    public Usuario? BuscarPorNombre(string nombreUsuario) =>
        _usuarios.FirstOrDefault(u => u.NombreUsuario.Equals(nombreUsuario, StringComparison.OrdinalIgnoreCase));

    public List<Usuario> ObtenerTodos() => _usuarios.ToList();

    public void Agregar(Usuario usuario)
    {
        usuario.Id = _usuarios.Max(u => u.Id) + 1;
        _usuarios.Add(usuario);
    }

    public void Eliminar(int id) => _usuarios.RemoveAll(u => u.Id == id);
    public bool TieneMovimientos(int id) => false;
}

public class VentaMemoryRepository : IVentaRepository
{
    private readonly List<Venta> _ventas = new();

    public void Agregar(Venta venta)
    {
        venta.Id = _ventas.Count + 1;
        foreach (var d in venta.Detalles) d.Producto.Stock -= d.Cantidad;
        _ventas.Add(venta);
    }

    public List<VentaResumen> ListarResumen() => _ventas
        .OrderByDescending(v => v.Id)
        .Select(v => new VentaResumen
        {
            Id = v.Id, Fecha = v.Fecha,
            Cliente = v.Cliente?.Nombre ?? "", Vendedor = v.Vendedor?.NombreUsuario ?? "",
            Unidades = v.Detalles.Sum(d => d.Cantidad),
            Subtotal = v.Subtotal, Descuento = v.Descuento, Total = v.Total
        }).ToList();

    public List<DetalleVenta> ObtenerDetalles(int ventaId) =>
        _ventas.FirstOrDefault(v => v.Id == ventaId)?.Detalles.ToList() ?? new();
}

public class CompraMemoryRepository : ICompraRepository
{
    private readonly List<Compra> _compras = new();

    public bool ExisteDocumento(int proveedorId, string numeroDocumento) =>
        _compras.Any(c => c.Proveedor?.Id == proveedorId && c.NumeroDocumento == numeroDocumento);

    public void Agregar(Compra compra)
    {
        compra.Id = _compras.Count + 1;
        foreach (var d in compra.Detalles) d.Producto.Stock += d.Cantidad;
        _compras.Add(compra);
    }

    public List<CompraResumen> ListarResumen() => _compras
        .OrderByDescending(c => c.Id)
        .Select(c => new CompraResumen
        {
            Id = c.Id, Fecha = c.Fecha,
            Proveedor = c.Proveedor?.Empresa ?? "", NumeroDocumento = c.NumeroDocumento,
            Usuario = c.Usuario?.NombreUsuario ?? "",
            Unidades = c.Detalles.Sum(d => d.Cantidad), Total = c.Total
        }).ToList();

    public List<DetalleCompra> ObtenerDetalles(int compraId) =>
        _compras.FirstOrDefault(c => c.Id == compraId)?.Detalles.ToList() ?? new();
}

// Calcula los reportes a partir de CUALQUIER IVentaRepository (otra muestra de DIP)
public class ReporteMemoryRepository : IReporteRepository
{
    private readonly IVentaRepository _ventas;
    public ReporteMemoryRepository(IVentaRepository ventas) => _ventas = ventas;

    private List<VentaResumen> EnRango(DateTime desde, DateTime hasta) => _ventas.ListarResumen()
        .Where(v => v.Fecha >= desde.Date && v.Fecha < hasta.Date.AddDays(1)).ToList();

    public List<ProductoVendido> ProductosMasVendidos(DateTime desde, DateTime hasta) => EnRango(desde, hasta)
        .SelectMany(v => _ventas.ObtenerDetalles(v.Id))
        .GroupBy(d => d.NombreProducto)
        .Select(g => new ProductoVendido { Producto = g.Key, Unidades = g.Sum(d => d.Cantidad), Importe = g.Sum(d => d.Subtotal) })
        .OrderByDescending(p => p.Unidades).ThenByDescending(p => p.Importe).ToList();

    public List<VentaResumen> VentasEnRango(DateTime desde, DateTime hasta) =>
        EnRango(desde, hasta).OrderBy(v => v.Fecha).ThenBy(v => v.Id).ToList();

    public List<VentasPorVendedor> VentasPorVendedor(DateTime desde, DateTime hasta) => EnRango(desde, hasta)
        .GroupBy(v => v.Vendedor)
        .Select(g => new VentasPorVendedor { Vendedor = g.Key, Ventas = g.Count(), Total = g.Sum(v => v.Total) })
        .OrderByDescending(v => v.Total).ToList();
}
