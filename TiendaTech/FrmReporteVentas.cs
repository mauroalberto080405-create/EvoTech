using System.Diagnostics;
using TiendaTech.Business;

namespace TiendaTech.UI;

// PANTALLA 12 - Reporte de ventas por rango de fechas (consultas con GROUP BY)
public class FrmReporteVentas : FormBase
{
    private readonly IReporteService _reportes;
    private readonly DateTimePicker _dtpDesde, _dtpHasta;
    private readonly DataGridView _gridProductos, _gridVendedores;
    private readonly Label _lblResumen;

    public FrmReporteVentas(IReporteService reportes)
    {
        _reportes = reportes;
        Text = "Reporte de ventas";
        ClientSize = new Size(860, 520);
        StartPosition = FormStartPosition.CenterScreen;

        // El rango inicial siempre incluye la última venta registrada (la pantalla no se abre vacía)
        var hoy = DateTime.Today;
        var rango = (Desde: new DateTime(hoy.Year, hoy.Month, 1), Hasta: hoy);
        try { rango = _reportes.RangoPorDefecto(); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }

        UiHelper.Etiqueta(this, "Desde:", 20, 22);
        _dtpDesde = new DateTimePicker { Left = 70, Top = 18, Width = 130, Format = DateTimePickerFormat.Short, Value = rango.Desde };
        Controls.Add(_dtpDesde);
        UiHelper.Etiqueta(this, "Hasta:", 220, 22);
        _dtpHasta = new DateTimePicker { Left = 270, Top = 18, Width = 130, Format = DateTimePickerFormat.Short, Value = rango.Hasta };
        Controls.Add(_dtpHasta);
        var btn = UiHelper.Boton(this, "Generar reporte", 420, 14, 150, color: Tema.Secundario);
        btn.Click += (_, _) => Generar();
        var btnExcel = UiHelper.Boton(this, "Exportar a Excel", 590, 14, 170, color: Tema.Excel);
        btnExcel.Click += BtnExportar_Click;

        UiHelper.Etiqueta(this, "Productos más vendidos", 20, 65).Font = new Font("Segoe UI", 10, FontStyle.Bold);
        _gridProductos = UiHelper.Grilla(this, 20, 90, 480, 370);
        UiHelper.Columna(_gridProductos, "Producto", "Producto", peso: 150);
        UiHelper.Columna(_gridProductos, "Unidades", "Unid.", peso: 45);
        UiHelper.Columna(_gridProductos, "Importe", "Importe S/", "N2", 70);
        UiHelper.Columna(_gridProductos, "Ganancia", "Ganancia S/", "N2", 70);

        UiHelper.Etiqueta(this, "Ventas por vendedor", 520, 65).Font = new Font("Segoe UI", 10, FontStyle.Bold);
        _gridVendedores = UiHelper.Grilla(this, 520, 90, 320, 370);
        UiHelper.Columna(_gridVendedores, "Vendedor", "Vendedor", peso: 80);
        UiHelper.Columna(_gridVendedores, "Ventas", "N° ventas", peso: 50);
        UiHelper.Columna(_gridVendedores, "Total", "Total S/", "N2", 65);
        UiHelper.Columna(_gridVendedores, "Ganancia", "Ganancia S/", "N2", 70);

        _lblResumen = UiHelper.Etiqueta(this, "", 20, 478);
        _lblResumen.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        _lblResumen.ForeColor = Tema.Secundario;

        Load += (_, _) => Generar();
    }

    // Exporta a .xlsx lo que corresponde al rango de fechas elegido
    private void BtnExportar_Click(object? sender, EventArgs e)
    {
        var desde = _dtpDesde.Value.Date;
        var hasta = _dtpHasta.Value.Date;
        try
        {
            if (_reportes.VentasPorVendedor(desde, hasta).Count == 0)
            {
                UiHelper.Error("No hay ventas entre las fechas seleccionadas. Cambie el rango y vuelva a intentar.");
                return;
            }
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); return; }
        catch (Exception ex) { UiHelper.ErrorBd(ex); return; }

        using var dialogo = new SaveFileDialog
        {
            Title = "Guardar reporte de ventas",
            Filter = "Libro de Excel (*.xlsx)|*.xlsx",
            DefaultExt = "xlsx",
            AddExtension = true,
            FileName = $"Reporte_ventas_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.xlsx"
        };
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            _reportes.ExportarExcel(desde, hasta, dialogo.FileName);
            if (UiHelper.Confirmar($"Reporte exportado correctamente:\n{dialogo.FileName}\n\n¿Desea abrirlo ahora?"))
                Process.Start(new ProcessStartInfo(dialogo.FileName) { UseShellExecute = true });
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (IOException ex)
        {
            UiHelper.Error("No se pudo guardar el archivo. Si ya está abierto en Excel, ciérrelo y vuelva a intentar.\n\n" + ex.Message);
        }
        catch (Exception ex) { UiHelper.Error("No se pudo generar el reporte en Excel:\n" + ex.Message); }
    }

    private void Generar()
    {
        try
        {
            var productos = _reportes.ProductosMasVendidos(_dtpDesde.Value, _dtpHasta.Value);
            var vendedores = _reportes.VentasPorVendedor(_dtpDesde.Value, _dtpHasta.Value);
            _gridProductos.DataSource = productos;
            _gridVendedores.DataSource = vendedores;
            _lblResumen.Text = $"Ventas en el periodo: {vendedores.Sum(v => v.Ventas)}     " +
                               $"Total vendido: S/ {vendedores.Sum(v => v.Total):N2}     " +
                               $"Ganancia: S/ {vendedores.Sum(v => v.Ganancia):N2}";
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }
}
