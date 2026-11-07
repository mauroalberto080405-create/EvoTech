using TiendaTech.Data;
using TiendaTech.Models;

namespace TiendaTech.Business;

public interface IProductoService
{
    List<Producto> Listar();
    List<Producto> ListarDisponibles();
    List<Producto> ListarStockBajo(int minimo);
    void Registrar(Producto producto);
    void Actualizar(Producto producto);
    void Eliminar(Producto? producto);
}

public class ProductoService : IProductoService
{
    private readonly IProductoRepository _repo;
    public ProductoService(IProductoRepository repo) => _repo = repo;

    public List<Producto> Listar() => _repo.ObtenerTodos();
    public List<Producto> ListarDisponibles() => _repo.ObtenerTodos().Where(p => p.Stock > 0).ToList();

    public List<Producto> ListarStockBajo(int minimo)
    {
        if (minimo < 0) throw new ArgumentException("El mínimo no puede ser negativo.");
        return _repo.ObtenerTodos().Where(p => p.Stock <= minimo).OrderBy(p => p.Stock).ToList();
    }

    public void Registrar(Producto p)
    {
        ValidarDatos(p);
        if (p.Stock < 0) throw new ArgumentException("El stock no puede ser negativo.");
        _repo.Agregar(p);
    }

    // Edita un producto existente (no crea uno nuevo). El stock no se cambia aquí.
    public void Actualizar(Producto p)
    {
        if (p.Id <= 0) throw new ArgumentException("Seleccione en la tabla el producto que desea actualizar.");
        if (_repo.ObtenerPorId(p.Id) == null) throw new ArgumentException("El producto seleccionado ya no existe.");
        ValidarDatos(p);
        _repo.Actualizar(p);
    }

    private static void ValidarDatos(Producto p)
    {
        p.Nombre = (p.Nombre ?? "").Trim();
        p.Marca = (p.Marca ?? "").Trim();
        if (p.Nombre.Length == 0) throw new ArgumentException("El nombre es obligatorio.");
        if (p.Precio <= 0) throw new ArgumentException("El precio debe ser mayor a 0.");
        if (p.Costo <= 0) throw new ArgumentException("El costo debe ser mayor a 0 (se usa al aumentar stock).");
    }

    public void Eliminar(Producto? p)
    {
        if (p == null) throw new ArgumentException("Seleccione un producto de la tabla.");
        if (_repo.TieneMovimientos(p.Id))
            throw new ArgumentException($"No se puede eliminar {p.Nombre}: tiene ventas o compras registradas.");
        _repo.Eliminar(p.Id);
    }
}
