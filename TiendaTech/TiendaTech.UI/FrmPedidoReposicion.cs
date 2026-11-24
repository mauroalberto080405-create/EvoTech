using System.Diagnostics;
using TiendaTech.Business;
using TiendaTech.Models;

namespace TiendaTech.UI;

// PANTALLA NUEVA (T4) - Pedido de reposición por proveedor (solo administrador)
// Lista lo que hay que comprar y a quién, y lo exporta a Excel como orden de pedido.
public class FrmPedidoReposicion : FormBase
{
    private const string Todos = "(Todos los proveedores)";

    private readonly IReposicionService _service;
    private readonly ComboBox _cboProveedor;
    private readonly DataGridView _grid;
    private readonly Label _lblResumen;
    private List<LineaReposicion> _lineas = new();

    public FrmPedidoReposicion(IReposicionService service)
    {
        _service = service;
        Text = "Pedido de reposición por proveedor";
        ClientSize = new Size(860, 500);
        StartPosition = FormStartPosition.CenterScreen;

        UiHelper.Etiqueta(this, "Pedido de reposición", 20, 15, true);
        var lblInfo = UiHelper.Etiqueta(this, "Productos en o bajo su stock mínimo. Cantidad sugerida = mínimo × 2 − stock (al menos 1).", 20, 55);
        lblInfo.ForeColor = Tema.Gris;

        UiHelper.Etiqueta(this, "Proveedor:", 20, 92);
        _cboProveedor = UiHelper.Combo(this, 100, 88, 280);
        _cboProveedor.SelectedIndexChanged += (_, _) => Filtrar();

        var btnActualizar = UiHelper.Boton(this, "Actualizar", 520, 84, 120, color: Tema.Secundario);
        btnActualizar.Click += (_, _) => Cargar();
        var btnExcel = UiHelper.Boton(this, "Exportar a Excel", 660, 84, 180, color: Tema.Excel);
        btnExcel.Click += BtnExportar_Click;

        _grid = UiHelper.Grilla(this, 20, 130, 820, 310);
        UiHelper.Columna(_grid, "Proveedor", "Proveedor", peso: 130);
        UiHelper.Columna(_grid, "Producto", "Producto", peso: 150);
        UiHelper.Columna(_grid, "Marca", "Marca", peso: 70);
        UiHelper.Columna(_grid, "Stock", "Stock", peso: 45);
        UiHelper.Columna(_grid, "StockMinimo", "Mínimo", peso: 50);
        UiHelper.Columna(_grid, "CantidadSugerida", "A pedir", peso: 55);
        UiHelper.Columna(_grid, "CostoUnitario", "Costo unit. S/", "N2", 70);
        UiHelper.Columna(_grid, "CostoEstimado", "Costo est. S/", "N2", 75);
        _grid.CellFormatting += ResaltarFilas;

        _lblResumen = UiHelper.Etiqueta(this, "", 20, 455);
        _lblResumen.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        _lblResumen.ForeColor = Tema.Oscuro;

        Load += (_, _) => Cargar();
    }

    private void Cargar()
    {
        try
        {
            _lineas = _service.Listar();

            var elegido = _cboProveedor.SelectedItem as string;
            _cboProveedor.Items.Clear();
            _cboProveedor.Items.Add(Todos);
            foreach (var p in _lineas.Select(l => l.Proveedor).Distinct()) _cboProveedor.Items.Add(p);
            _cboProveedor.SelectedItem = elegido != null && _cboProveedor.Items.Contains(elegido) ? elegido : Todos;

            Filtrar();
        }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }

    private void Filtrar()
    {
        var proveedor = _cboProveedor.SelectedItem as string;
        var vista = proveedor == null || proveedor == Todos
            ? _lineas
            : _lineas.Where(l => l.Proveedor == proveedor).ToList();

        _grid.DataSource = vista;
        _lblResumen.ForeColor = vista.Count == 0 ? Color.ForestGreen : Tema.Oscuro;
        _lblResumen.Text = vista.Count == 0
            ? "Todo el inventario está sobre su mínimo. No hay nada que pedir."
            : $"{vista.Count} producto(s) · {vista.Sum(l => l.CantidadSugerida)} unidades · " +
              $"{vista.Select(l => l.Proveedor).Distinct().Count()} proveedor(es) · costo estimado S/ {vista.Sum(l => l.CostoEstimado):N2}";
    }

    // Agotados en rojo; productos sin proveedor asignado en gris (hay que asignarlo en Productos)
    private void ResaltarFilas(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.CellStyle == null || _grid.Rows[e.RowIndex].DataBoundItem is not LineaReposicion l) return;
        if (l.Stock == 0) e.CellStyle.ForeColor = Color.Firebrick;
        else if (l.ProveedorId == null) e.CellStyle.ForeColor = Tema.Gris;
    }

    private void BtnExportar_Click(object? sender, EventArgs e)
    {
        using var dialogo = new SaveFileDialog
        {
            Title = "Guardar pedido de reposición",
            Filter = "Libro de Excel (*.xlsx)|*.xlsx",
            FileName = $"Pedido_reposicion_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
        };
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            _service.ExportarExcel(dialogo.FileName);
            if (UiHelper.Confirmar($"Pedido exportado correctamente:\n{dialogo.FileName}\n\n¿Desea abrirlo ahora?"))
                Process.Start(new ProcessStartInfo(dialogo.FileName) { UseShellExecute = true });
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (IOException ex)
        {
            UiHelper.Error("No se pudo guardar el archivo. Si ya está abierto en Excel, ciérrelo y vuelva a intentar.\n\n" + ex.Message);
        }
        catch (Exception ex) { UiHelper.Error("No se pudo generar el pedido en Excel:\n" + ex.Message); }
    }
}
