using TiendaTech.Business;
using TiendaTech.Models;

namespace TiendaTech.UI;

// PANTALLA 6 - Alertas de stock bajo (consulta tabla Producto)
public class FrmStockBajo : FormBase
{
    private readonly IProductoService _service;
    private readonly NumericUpDown _nudMinimo;
    private readonly DataGridView _grid;
    private readonly Label _lblResumen;

    public FrmStockBajo(IProductoService service)
    {
        _service = service;
        Text = "Alertas de stock bajo";
        ClientSize = new Size(700, 460);
        StartPosition = FormStartPosition.CenterScreen;

        UiHelper.Etiqueta(this, "Stock menor o igual a:", 20, 22);
        _nudMinimo = new NumericUpDown { Left = 300, Top = 18, Width = 70, Maximum = 1000, Value = 10 };
        Controls.Add(_nudMinimo);
        var btn = UiHelper.Boton(this, "Consultar", 390, 14, 120, color: Tema.Secundario);
        btn.Click += (_, _) => Cargar();

        _grid = UiHelper.Grilla(this, 20, 60, 660, 340);
        UiHelper.Columna(_grid, "Id", "Id", peso: 30);
        UiHelper.Columna(_grid, "Nombre", "Producto", peso: 150);
        UiHelper.Columna(_grid, "Marca", "Marca", peso: 70);
        UiHelper.Columna(_grid, "Stock", "Stock", peso: 45);
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
            var lista = _service.ListarStockBajo((int)_nudMinimo.Value);
            _grid.DataSource = lista;
            _lblResumen.Text = lista.Count == 0
                ? "Todo el inventario está sobre el mínimo."
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
