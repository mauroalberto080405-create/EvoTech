using TiendaTech.Business;
using TiendaTech.Models;

namespace TiendaTech.UI;

// PANTALLA 10 - Historial de ventas (consulta Venta + DetalleVenta con JOIN)
public class FrmHistorialVentas : FormBase
{
    private readonly IVentaService _ventas;
    private readonly DataGridView _gridVentas, _gridDetalle;
    private readonly Label _lblResumen;

    public FrmHistorialVentas(IVentaService ventas)
    {
        _ventas = ventas;
        Text = "Historial de ventas";
        ClientSize = new Size(820, 560);
        StartPosition = FormStartPosition.CenterScreen;

        UiHelper.Etiqueta(this, "Historial de ventas", 20, 12, true);
        var btnActualizar = UiHelper.Boton(this, "Actualizar", 680, 16, 120, color: Tema.Secundario);
        btnActualizar.Click += (_, _) => Cargar();

        _gridVentas = UiHelper.Grilla(this, 20, 60, 780, 260);
        UiHelper.Columna(_gridVentas, "Id", "N°", peso: 35);
        UiHelper.Columna(_gridVentas, "Fecha", "Fecha", "dd/MM/yyyy HH:mm", 110);
        UiHelper.Columna(_gridVentas, "Cliente", "Cliente", peso: 130);
        UiHelper.Columna(_gridVentas, "Vendedor", "Vendedor", peso: 80);
        UiHelper.Columna(_gridVentas, "Unidades", "Unid.", peso: 45);
        UiHelper.Columna(_gridVentas, "Subtotal", "Subtotal S/", "N2", 75);
        UiHelper.Columna(_gridVentas, "Descuento", "Desc. S/", "N2", 65);
        UiHelper.Columna(_gridVentas, "Total", "Total S/", "N2", 75);
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
            _lblResumen.Text = $"Ventas registradas: {lista.Count}     Total vendido: S/ {lista.Sum(v => v.Total):N2}";
            MostrarDetalle();
        }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }

    private void MostrarDetalle()
    {
        if (_gridVentas.CurrentRow?.DataBoundItem is VentaResumen v)
            _gridDetalle.DataSource = _ventas.ObtenerDetalles(v.Id);
        else
            _gridDetalle.DataSource = null;
    }
}
