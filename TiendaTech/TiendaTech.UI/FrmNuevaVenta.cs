using System.ComponentModel;
using TiendaTech.Business;
using TiendaTech.Models;

namespace TiendaTech.UI;

// PANTALLA 9 - Nueva venta (tablas Venta + DetalleVenta, descuenta stock de Producto)
public class FrmNuevaVenta : FormBase
{
    private readonly IVentaService _ventas;
    private readonly IClienteService _clientes;
    private readonly IProductoService _productos;

    private readonly ComboBox _cboCliente, _cboProducto;
    private readonly NumericUpDown _nudCantidad;
    private readonly DataGridView _grid;
    private readonly Label _lblSubtotal, _lblDescuento, _lblTotal;
    private readonly BindingList<DetalleVenta> _carrito = new();

    public FrmNuevaVenta(IVentaService ventas, IClienteService clientes, IProductoService productos)
    {
        _ventas = ventas;
        _clientes = clientes;
        _productos = productos;

        Text = "Nueva venta";
        ClientSize = new Size(720, 520);
        StartPosition = FormStartPosition.CenterScreen;

        UiHelper.Etiqueta(this, "Cliente:", 20, 20);
        _cboCliente = UiHelper.Combo(this, 100, 16, 250);

        UiHelper.Etiqueta(this, "Producto:", 20, 60);
        _cboProducto = UiHelper.Combo(this, 100, 56, 350);

        UiHelper.Etiqueta(this, "Cantidad:", 465, 60);
        _nudCantidad = new NumericUpDown { Left = 535, Top = 56, Width = 60, Minimum = 1, Maximum = 1000, Value = 1 };
        Controls.Add(_nudCantidad);

        var btnAgregar = UiHelper.Boton(this, "Agregar", 610, 53, 90, color: Tema.Secundario);
        btnAgregar.Click += BtnAgregar_Click;

        _grid = UiHelper.Grilla(this, 20, 100, 680, 280);
        UiHelper.Columna(_grid, "NombreProducto", "Producto", peso: 200);
        UiHelper.Columna(_grid, "Cantidad", "Cant.", peso: 50);
        UiHelper.Columna(_grid, "PrecioUnitario", "Precio S/", "N2", 70);
        UiHelper.Columna(_grid, "Subtotal", "Subtotal S/", "N2", 70);
        _grid.DataSource = _carrito;

        _lblSubtotal = UiHelper.Etiqueta(this, "Subtotal: S/ 0.00", 480, 395);
        _lblDescuento = UiHelper.Etiqueta(this, "Descuento: S/ 0.00", 480, 420);
        _lblTotal = UiHelper.Etiqueta(this, "TOTAL: S/ 0.00", 480, 445);
        _lblTotal.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        _lblTotal.ForeColor = Tema.Primario;

        var btnQuitar = UiHelper.Boton(this, "Quitar seleccionado", 20, 395, 160, color: Tema.Gris);
        btnQuitar.Click += BtnQuitar_Click;
        var btnFinalizar = UiHelper.Boton(this, "Finalizar venta", 20, 440, 160, 40);
        btnFinalizar.Click += BtnFinalizar_Click;

        Load += (_, _) => CargarCombos();
    }

    private void CargarCombos()
    {
        try
        {
            _cboCliente.DataSource = _clientes.Listar();
            _cboProducto.DataSource = _productos.ListarDisponibles();
        }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }

    private void BtnAgregar_Click(object? sender, EventArgs e)
    {
        if (_cboProducto.SelectedItem is not Producto p) { UiHelper.Error("Seleccione un producto."); return; }

        int cantidad = (int)_nudCantidad.Value;
        var existente = _carrito.FirstOrDefault(d => d.Producto.Id == p.Id);
        int total = cantidad + (existente?.Cantidad ?? 0);
        if (total > p.Stock) { UiHelper.Error($"Stock insuficiente. Disponible: {p.Stock}."); return; }

        if (existente != null) { existente.Cantidad = total; _carrito.ResetBindings(); }
        else _carrito.Add(new DetalleVenta { Producto = p, Cantidad = cantidad });
        ActualizarTotales();
    }

    private void BtnQuitar_Click(object? sender, EventArgs e)
    {
        if (_grid.CurrentRow?.DataBoundItem is DetalleVenta d)
        {
            _carrito.Remove(d);
            ActualizarTotales();
        }
    }

    private void ActualizarTotales()
    {
        decimal subtotal = _carrito.Sum(d => d.Subtotal);
        decimal descuento = _ventas.CalcularDescuento(subtotal);
        _lblSubtotal.Text = $"Subtotal: S/ {subtotal:N2}";
        _lblDescuento.Text = $"Descuento: S/ {descuento:N2}";
        _lblTotal.Text = $"TOTAL: S/ {subtotal - descuento:N2}";
    }

    private void BtnFinalizar_Click(object? sender, EventArgs e)
    {
        try
        {
            var venta = _ventas.Registrar(_cboCliente.SelectedItem as Cliente, _carrito.ToList());
            MessageBox.Show($"Venta #{venta.Id} registrada en la base de datos.\n" +
                            $"Cliente: {venta.Cliente?.Nombre}\nTotal: S/ {venta.Total:N2}",
                            "Venta registrada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _carrito.Clear();
            ActualizarTotales();
            CargarCombos(); // vuelve a leer el stock desde la BD
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (Exception ex) { UiHelper.Error(ex.Message); CargarCombos(); }
    }
}
