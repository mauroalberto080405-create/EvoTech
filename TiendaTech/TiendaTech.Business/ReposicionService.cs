using TiendaTech.Data;
using TiendaTech.Models;

namespace TiendaTech.Business;

public interface IReposicionService
{
    // Productos por reponer con la cantidad sugerida ya calculada
    List<LineaReposicion> Listar();
    // Genera el pedido en Excel: una hoja de resumen y una hoja por proveedor
    void ExportarExcel(string rutaArchivo);
}

public class ReposicionService : IReposicionService
{
    private readonly IReposicionRepository _repo;
    public ReposicionService(IReposicionRepository repo) => _repo = repo;

    // Regla del negocio: reponer hasta el doble del mínimo, y pedir al menos 1 unidad.
    // Ej.: mínimo 5, stock 2 -> 5 x 2 - 2 = 8 unidades.
    public static int CalcularCantidadSugerida(int stock, int stockMinimo) =>
        Math.Max(1, stockMinimo * 2 - Math.Max(0, stock));

    public List<LineaReposicion> Listar()
    {
        var lista = _repo.ProductosPorReponer();
        foreach (var l in lista)
            l.CantidadSugerida = CalcularCantidadSugerida(l.Stock, l.StockMinimo);
        return lista;
    }

    public void ExportarExcel(string rutaArchivo)
    {
        if (string.IsNullOrWhiteSpace(rutaArchivo)) throw new ArgumentException("Elija dónde guardar el archivo.");

        var lineas = Listar();
        if (lineas.Count == 0)
            throw new ArgumentException("Todo el inventario está sobre su mínimo. No hay nada que pedir.");

        var grupos = lineas.GroupBy(l => (l.ProveedorId, l.Proveedor, l.RucProveedor, l.TelefonoProveedor)).ToList();

        var resumen = new HojaExcel("Resumen",
            new[] { "Proveedor", "RUC", "Teléfono", "Productos", "Unidades", "Costo estimado S/" },
            new[] { 30.0, 14.0, 12.0, 11.0, 11.0, 18.0 });
        foreach (var g in grupos)
            resumen.Filas.Add(new object[] { g.Key.Proveedor, g.Key.RucProveedor, g.Key.TelefonoProveedor,
                                             g.Count(), g.Sum(l => l.CantidadSugerida), g.Sum(l => l.CostoEstimado) });
        resumen.Filas.Add(new object[] { "TOTAL", "", "", lineas.Count, lineas.Sum(l => l.CantidadSugerida), lineas.Sum(l => l.CostoEstimado) });
        // Pie de página: el encabezado naranja siempre va en la fila 1, así que el título va al final
        resumen.Filas.Add(new object[] { "", "", "", "", "", "" });
        resumen.Filas.Add(new object[] { "Pedido de reposición – EvoTech · generado el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), "", "", "", "", "" });

        var hojas = new List<HojaExcel> { resumen };
        var usados = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { resumen.Nombre };
        foreach (var g in grupos)
        {
            var hoja = new HojaExcel(NombreHoja(g.Key.Proveedor, usados),
                new[] { "Producto", "Marca", "Stock", "Mínimo", "Cantidad a pedir", "Costo unit. S/", "Costo estimado S/" },
                new[] { 30.0, 14.0, 9.0, 9.0, 16.0, 14.0, 18.0 });
            foreach (var l in g)
                hoja.Filas.Add(new object[] { l.Producto, l.Marca, l.Stock, l.StockMinimo, l.CantidadSugerida, l.CostoUnitario, l.CostoEstimado });
            hoja.Filas.Add(new object[] { "TOTAL", "", "", "", g.Sum(l => l.CantidadSugerida), "", g.Sum(l => l.CostoEstimado) });
            hojas.Add(hoja);
        }

        ExportadorExcel.Guardar(rutaArchivo, hojas);
    }

    // Excel no acepta : \ / ? * [ ] en el nombre de una hoja, ni más de 31 caracteres, ni nombres repetidos
    private static string NombreHoja(string proveedor, HashSet<string> usados)
    {
        var limpio = new string(proveedor.Select(c => ":\\/?*[]".Contains(c) ? ' ' : c).ToArray()).Trim();
        if (limpio.Length == 0) limpio = "Proveedor";
        if (limpio.Length > 28) limpio = limpio[..28].Trim();

        var nombre = limpio;
        for (int i = 2; !usados.Add(nombre); i++) nombre = $"{limpio} {i}";
        return nombre;
    }
}
