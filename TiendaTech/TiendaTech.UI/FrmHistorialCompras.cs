using TiendaTech.Business;
using TiendaTech.Models;

namespace TiendaTech.UI;

// PANTALLA 11 - Historial de compras (consulta Compra + DetalleCompra con JOIN)
public class FrmHistorialCompras : FormBase
{
    private readonly ICompraService _compras;
    private readonly DataGridView _gridCompras, _gridDetalle;
    private readonly Label _lblResumen;

    public FrmHistorialCompras(ICompraService compras)
    {
        _compras = compras;
        Text = "Historial de compras";
        ClientSize = new Size(820, 560);
        StartPosition = FormStartPosition.CenterScreen;

        UiHelper.Etiqueta(this, "Historial de compras", 20, 12, true);
        var btnActualizar = UiHelper.Boton(this, "Actualizar", 680, 16, 120, color: Tema.Secundario);
        btnActualizar.Click += (_, _) => Cargar();

        _gridCompras = UiHelper.Grilla(this, 20, 60, 780, 260);
        UiHelper.Columna(_gridCompras, "Id", "N°", peso: 35);
        UiHelper.Columna(_gridCompras, "Fecha", "Fecha", "dd/MM/yyyy HH:mm", 110);
        UiHelper.Columna(_gridCompras, "Proveedor", "Proveedor", peso: 170);
        UiHelper.Columna(_gridCompras, "Usuario", "Registró", peso: 80);
        UiHelper.Columna(_gridCompras, "Unidades", "Unid.", peso: 45);
        UiHelper.Columna(_gridCompras, "Total", "Total S/", "N2", 75);
        _gridCompras.SelectionChanged += (_, _) => MostrarDetalle();

        UiHelper.Etiqueta(this, "Detalle de la compra seleccionada:", 20, 332);
        _gridDetalle = UiHelper.Grilla(this, 20, 355, 780, 150);
        UiHelper.Columna(_gridDetalle, "NombreProducto", "Producto", peso: 200);
        UiHelper.Columna(_gridDetalle, "Cantidad", "Cant.", peso: 50);
        UiHelper.Columna(_gridDetalle, "CostoUnitario", "Costo S/", "N2", 70);
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
            var lista = _compras.ListarHistorial();
            _gridCompras.DataSource = lista;
            _lblResumen.Text = $"Compras registradas: {lista.Count}     Total invertido: S/ {lista.Sum(c => c.Total):N2}";
            MostrarDetalle();
        }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }

    private void MostrarDetalle()
    {
        if (_gridCompras.CurrentRow?.DataBoundItem is CompraResumen c)
            _gridDetalle.DataSource = _compras.ObtenerDetalles(c.Id);
        else
            _gridDetalle.DataSource = null;
    }
}
