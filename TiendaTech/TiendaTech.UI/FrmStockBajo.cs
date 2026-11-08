using TiendaTech.Business;
using TiendaTech.Models;

namespace TiendaTech.UI;

// PANTALLA 6 - Alertas de stock bajo (consulta tabla Producto)
public class FrmStockBajo : FormBase
{
    private readonly IProductoService _service;
    private readonly DataGridView _grid;
    private readonly Label _lblResumen;

    public FrmStockBajo(IProductoService service)
    {
        _service = service;
        Text = "Alertas de stock bajo";
        ClientSize = new Size(700, 460);
        StartPosition = FormStartPosition.CenterScreen;

        var lblInfo = UiHelper.Etiqueta(this, "Se listan los productos cuyo stock es menor o igual a su stock mínimo (se define en Productos).", 20, 22);
        lblInfo.ForeColor = Tema.Gris;
        var btn = UiHelper.Boton(this, "Actualizar", 560, 14, 120, color: Tema.Secundario);
        btn.Click += (_, _) => Cargar();

        _grid = UiHelper.Grilla(this, 20, 60, 660, 340);
        UiHelper.Columna(_grid, "Id", "Id", peso: 30);
        UiHelper.Columna(_grid, "Nombre", "Producto", peso: 150);
        UiHelper.Columna(_grid, "Marca", "Marca", peso: 70);
        UiHelper.Columna(_grid, "Stock", "Stock", peso: 45);
        UiHelper.Columna(_grid, "StockMinimo", "Mínimo", peso: 45);
        UiHelper.Columna(_grid, "ProveedorNombre", "Proveedor", peso: 150);
        _grid.CellFormatting += PintarAgotados;

        _lblResumen = UiHelper.Etiqueta(this, "", 20, 415);
        _lblResumen.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        _lblResumen.ForeColor = Color.Firebrick;

        Load += (_, _) => Cargar();
    }

    private void Cargar()
    {
        try
        {
            var lista = _service.ListarStockBajo();
            _grid.DataSource = lista;
            _lblResumen.Text = lista.Count == 0
                ? "Todo el inventario está sobre su mínimo."
                : $"{lista.Count} producto(s) necesitan reposición.";
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }

    // Los productos agotados (stock 0) se pintan en rojo
    private void PintarAgotados(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex >= 0 && _grid.Rows[e.RowIndex].DataBoundItem is Producto p && p.Stock == 0)
            e.CellStyle.ForeColor = Color.Firebrick;
    }
}
