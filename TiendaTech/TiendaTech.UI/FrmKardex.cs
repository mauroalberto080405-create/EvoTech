using TiendaTech.Business;
using TiendaTech.Models;

namespace TiendaTech.UI;

// Kardex de producto: historial de entradas (compras), salidas (ventas) y anulaciones, con el saldo después de cada movimiento.
// Se abre desde Productos con el botón "Ver movimientos".
public class FrmKardex : FormBase
{
    private readonly IKardexService _service;
    private readonly int _productoId;
    private readonly DataGridView _grid;
    private readonly Label _lblTitulo, _lblResumen;

    public FrmKardex(IKardexService service, int productoId)
    {
        _service = service;
        _productoId = productoId;
        Text = "Kardex de producto";
        ClientSize = new Size(760, 480);
        StartPosition = FormStartPosition.CenterParent;

        _lblTitulo = UiHelper.Etiqueta(this, "", 20, 14, true);
        var btnActualizar = UiHelper.Boton(this, "Actualizar", 620, 14, 120, color: Tema.Secundario);
        btnActualizar.Click += (_, _) => Cargar();

        _grid = UiHelper.Grilla(this, 20, 60, 720, 360);
        UiHelper.Columna(_grid, "Fecha", "Fecha", "dd/MM/yyyy HH:mm", 100);
        UiHelper.Columna(_grid, "Tipo", "Movimiento", peso: 120);
        UiHelper.Columna(_grid, "Documento", "Documento", peso: 170);
        UiHelper.Columna(_grid, "Entrada", "Entrada", peso: 55);
        UiHelper.Columna(_grid, "Salida", "Salida", peso: 55);
        UiHelper.Columna(_grid, "Saldo", "Saldo", peso: 55);
        _grid.CellFormatting += PintarMovimiento;

        _lblResumen = UiHelper.Etiqueta(this, "", 20, 435);
        _lblResumen.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        _lblResumen.ForeColor = Tema.Secundario;

        Load += (_, _) => Cargar();
    }

    private void Cargar()
    {
        try
        {
            var k = _service.Consultar(_productoId);
            _lblTitulo.Text = $"Kardex: {k.Producto.Nombre} ({k.Producto.Marca})";
            _grid.DataSource = k.Movimientos;
            _lblResumen.Text = $"Entradas: {k.TotalEntradas}   Salidas: {k.TotalSalidas}   Stock actual: {k.Producto.Stock}";
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }

    // Salidas en rojo, anulaciones en turquesa, saldo inicial en gris
    private void PintarMovimiento(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || _grid.Rows[e.RowIndex].DataBoundItem is not MovimientoKardex m) return;
        if (m.Salida > 0) e.CellStyle.ForeColor = Color.Firebrick;
        else if (m.Tipo == "Anulación de venta") e.CellStyle.ForeColor = Tema.Secundario;
        else if (m.Tipo == "Saldo inicial") e.CellStyle.ForeColor = Tema.Gris;
    }
}
