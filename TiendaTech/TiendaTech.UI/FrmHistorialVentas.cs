using TiendaTech.Business;
using TiendaTech.Models;

namespace TiendaTech.UI;

// PANTALLA 10 - Historial de ventas (consulta Venta + DetalleVenta con JOIN)
public class FrmHistorialVentas : FormBase
{
    private readonly IVentaService _ventas;
    private readonly DataGridView _gridVentas, _gridDetalle;
    private readonly Label _lblResumen;
    private readonly Button _btnAnular;

    public FrmHistorialVentas(IVentaService ventas)
    {
        _ventas = ventas;
        Text = "Historial de ventas";
        ClientSize = new Size(820, 560);
        StartPosition = FormStartPosition.CenterScreen;

        UiHelper.Etiqueta(this, "Historial de ventas", 20, 12, true);
        var btnActualizar = UiHelper.Boton(this, "Actualizar", 680, 16, 120, color: Tema.Secundario);
        btnActualizar.Click += (_, _) => Cargar();
        _btnAnular = UiHelper.Boton(this, "Anular venta", 530, 16, 135, color: Tema.Peligro);
        _btnAnular.Enabled = false;
        _btnAnular.Click += (_, _) => AnularSeleccionada();

        _gridVentas = UiHelper.Grilla(this, 20, 60, 780, 260);
        UiHelper.Columna(_gridVentas, "Id", "N°", peso: 35);
        UiHelper.Columna(_gridVentas, "Fecha", "Fecha", "dd/MM/yyyy HH:mm", 110);
        UiHelper.Columna(_gridVentas, "Cliente", "Cliente", peso: 130);
        UiHelper.Columna(_gridVentas, "Vendedor", "Vendedor", peso: 80);
        UiHelper.Columna(_gridVentas, "Unidades", "Unid.", peso: 45);
        UiHelper.Columna(_gridVentas, "Subtotal", "Subtotal S/", "N2", 75);
        UiHelper.Columna(_gridVentas, "Descuento", "Desc. S/", "N2", 65);
        UiHelper.Columna(_gridVentas, "Total", "Total S/", "N2", 75);
        UiHelper.Columna(_gridVentas, "Estado", "Estado", peso: 60);
        _gridVentas.DataBindingComplete += (_, _) => MarcarAnuladas();
        _gridVentas.SelectionChanged += (_, _) => MostrarDetalle();

        UiHelper.Etiqueta(this, "Detalle de la venta seleccionada:", 20, 332);
        _gridDetalle = UiHelper.Grilla(this, 20, 355, 780, 150);
        UiHelper.Columna(_gridDetalle, "NombreProducto", "Producto", peso: 200);
        UiHelper.Columna(_gridDetalle, "Cantidad", "Cant.", peso: 50);
        UiHelper.Columna(_gridDetalle, "PrecioUnitario", "Precio S/", "N2", 70);
        UiHelper.Columna(_gridDetalle, "Subtotal", "Subtotal S/", "N2", 70);

        _lblResumen = UiHelper.Etiqueta(this, "", 20, 520);
        _lblResumen.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        _lblResumen.ForeColor = Tema.Secundario;

        Load += (_, _) => Cargar();
    }

    private void Cargar()
    {
        try
        {
            var lista = _ventas.ListarHistorial();
            _gridVentas.DataSource = lista;
            var activas = lista.Where(v => !v.Anulada).ToList(); // las anuladas no cuentan en el total
            var anuladas = lista.Count - activas.Count;
            _lblResumen.Text = $"Ventas registradas: {activas.Count}     Total vendido: S/ {activas.Sum(v => v.Total):N2}" +
                               (anuladas > 0 ? $"     Anuladas: {anuladas}" : "");
            MostrarDetalle();
        }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }

    private void MostrarDetalle()
    {
        if (_gridVentas.CurrentRow?.DataBoundItem is VentaResumen v)
        {
            _gridDetalle.DataSource = _ventas.ObtenerDetalles(v.Id);
            _btnAnular.Enabled = !v.Anulada; // una venta ya anulada no se puede anular otra vez
        }
        else
        {
            _gridDetalle.DataSource = null;
            _btnAnular.Enabled = false;
        }
    }

    // Las ventas anuladas se ven en rojo para distinguirlas de las activas
    private void MarcarAnuladas()
    {
        foreach (DataGridViewRow fila in _gridVentas.Rows)
            if (fila.DataBoundItem is VentaResumen v && v.Anulada)
                fila.DefaultCellStyle.ForeColor = Tema.Peligro;
    }

    private void AnularSeleccionada()
    {
        if (_gridVentas.CurrentRow?.DataBoundItem is not VentaResumen v) return;
        if (v.Anulada) { UiHelper.Error("Esa venta ya está anulada."); return; }

        if (!UiHelper.Confirmar($"¿Anular la venta N° {v.Id} por S/ {v.Total:N2}?\n\n" +
                                "Se devolverá el stock de los productos y la venta quedará marcada como anulada.")) return;
        try
        {
            _ventas.Anular(v.Id);
            Cargar();
            MessageBox.Show($"La venta N° {v.Id} fue anulada y el stock fue devuelto.",
                "Venta anulada", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (InvalidOperationException ex) { UiHelper.Error(ex.Message); Cargar(); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }
}
