using System.ComponentModel;
using TiendaTech.Business;
using TiendaTech.Models;

namespace TiendaTech.UI;

// PANTALLA 8 - Aumentar stock (tablas Compra + DetalleCompra)
// Registra el ingreso de mercadería del proveedor y AUMENTA el stock en una sola transacción.
// El costo NO se escribe: se toma del producto (columna Producto.Costo).
public class FrmCompras : FormBase
{
    private readonly ICompraService _compras;
    private readonly IProveedorService _proveedores;
    private readonly IProductoService _productos;

    private readonly ComboBox _cboProveedor, _cboProducto;
    private readonly NumericUpDown _nudCantidad;
    private readonly Label _lblCosto;
    private readonly DataGridView _grid;
    private readonly Label _lblTotal;
    private readonly BindingList<DetalleCompra> _detalle = new();

    public FrmCompras(ICompraService compras, IProveedorService proveedores, IProductoService productos)
    {
        _compras = compras;
        _proveedores = proveedores;
        _productos = productos;

        Text = "Aumentar stock";
        ClientSize = new Size(720, 520);
        StartPosition = FormStartPosition.CenterScreen;

        UiHelper.Etiqueta(this, "Proveedor:", 20, 20);
        _cboProveedor = UiHelper.Combo(this, 100, 16, 280);

        UiHelper.Etiqueta(this, "Producto:", 20, 60);
        _cboProducto = UiHelper.Combo(this, 100, 56, 280);
        _cboProducto.DisplayMember = nameof(Producto.Nombre); // solo el nombre
        _cboProducto.ValueMember = nameof(Producto.Id);       // el Id se conserva para la BD
        _cboProducto.SelectedIndexChanged += (_, _) => MostrarCosto();

        UiHelper.Etiqueta(this, "Cant.:", 395, 60);
        _nudCantidad = new NumericUpDown { Left = 440, Top = 56, Width = 60, Minimum = 1, Maximum = 10000, Value = 1 };
        Controls.Add(_nudCantidad);
        _lblCosto = UiHelper.Etiqueta(this, "Costo unit.: S/ 0.00", 515, 60);

        var btnAgregar = UiHelper.Boton(this, "Agregar", 610, 14, 90, color: Tema.Secundario);
        btnAgregar.Click += BtnAgregar_Click;

        _grid = UiHelper.Grilla(this, 20, 100, 680, 280);
        UiHelper.Columna(_grid, "NombreProducto", "Producto", peso: 200);
        UiHelper.Columna(_grid, "Cantidad", "Cant.", peso: 50);
        UiHelper.Columna(_grid, "CostoUnitario", "Costo S/", "N2", 70);
        UiHelper.Columna(_grid, "Subtotal", "Subtotal S/", "N2", 70);
        _grid.DataSource = _detalle;

        _lblTotal = UiHelper.Etiqueta(this, "COSTO TOTAL: S/ 0.00", 430, 400);
        _lblTotal.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        _lblTotal.ForeColor = Tema.Primario;

        var btnQuitar = UiHelper.Boton(this, "Quitar seleccionado", 20, 395, 160, color: Tema.Gris);
        btnQuitar.Click += (_, _) =>
        {
            if (_grid.CurrentRow?.DataBoundItem is DetalleCompra d) { _detalle.Remove(d); ActualizarTotal(); }
        };
        var btnRegistrar = UiHelper.Boton(this, "Aumentar stock", 20, 440, 160, 40);
        btnRegistrar.Click += BtnRegistrar_Click;

        Load += (_, _) => CargarCombos();
    }

    private void CargarCombos()
    {
        try
        {
            _cboProveedor.DataSource = _proveedores.Listar();
            _cboProducto.DataSource = _productos.Listar();
            MostrarCosto();
        }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }

    private void MostrarCosto() =>
        _lblCosto.Text = _cboProducto.SelectedItem is Producto p ? $"Costo unit.: S/ {p.Costo:N2}" : "Costo unit.: -";

    private void BtnAgregar_Click(object? sender, EventArgs e)
    {
        if (_cboProducto.SelectedItem is not Producto p) { UiHelper.Error("Seleccione un producto."); return; }
        if (p.Costo <= 0)
        {
            UiHelper.Error($"El producto {p.Nombre} no tiene un costo registrado.\nRegístrelo en la pantalla Productos (botón Actualizar).");
            return;
        }

        int cantidad = (int)_nudCantidad.Value;
        var existente = _detalle.FirstOrDefault(d => d.Producto.Id == p.Id);
        if (existente != null)
        {
            existente.Cantidad += cantidad;
            _detalle.ResetBindings();
        }
        else _detalle.Add(new DetalleCompra { Producto = p, Cantidad = cantidad, CostoUnitario = p.Costo });
        ActualizarTotal();
    }

    private void ActualizarTotal() => _lblTotal.Text = $"COSTO TOTAL: S/ {_detalle.Sum(d => d.Subtotal):N2}";

    private void BtnRegistrar_Click(object? sender, EventArgs e)
    {
        try
        {
            // El servicio vuelve a leer el costo de la BD: lo que muestre la pantalla no se usa para guardar
            var ingreso = _compras.Registrar(_cboProveedor.SelectedItem as Proveedor, _detalle.ToList());
            MessageBox.Show($"Stock aumentado correctamente (ingreso N° {ingreso.Id}).\n" +
                            $"Proveedor: {ingreso.Proveedor?.Empresa}\n" +
                            $"Unidades: {ingreso.Detalles.Sum(d => d.Cantidad)}\nCosto total: S/ {ingreso.Total:N2}",
                            "Aumentar stock", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _detalle.Clear();
            ActualizarTotal();
            CargarCombos();
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); CargarCombos(); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }
}
