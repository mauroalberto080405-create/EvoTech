using System.ComponentModel;
using TiendaTech.Business;
using TiendaTech.Models;

namespace TiendaTech.UI;

// PANTALLA 8 - Aumentar stock (tablas Compra + DetalleCompra)
// Registra el ingreso de mercadería del proveedor y AUMENTA el stock en una sola transacción.
// El costo NO se escribe: se toma del producto (columna Producto.Costo).
// v3: se registra el N° de factura/guía del proveedor y solo se listan los productos de ESE proveedor.
public class FrmCompras : FormBase
{
    private readonly ICompraService _compras;
    private readonly IProveedorService _proveedores;

    private readonly ComboBox _cboProveedor, _cboProducto;
    private readonly TextBox _txtDocumento;
    private readonly NumericUpDown _nudCantidad;
    private readonly Label _lblCosto, _lblFiltro;
    private readonly DataGridView _grid;
    private readonly Label _lblTotal;
    private readonly BindingList<DetalleCompra> _detalle = new();
    private bool _cargando; // evita reaccionar al cambio de proveedor mientras se llenan los combos

    public FrmCompras(ICompraService compras, IProveedorService proveedores)
    {
        _compras = compras;
        _proveedores = proveedores;

        Text = "Aumentar stock";
        ClientSize = new Size(720, 520);
        StartPosition = FormStartPosition.CenterScreen;

        UiHelper.Etiqueta(this, "Proveedor:", 20, 20);
        _cboProveedor = UiHelper.Combo(this, 100, 16, 280);
        _cboProveedor.SelectedIndexChanged += (_, _) => CambioProveedor();

        // N° de la factura o guía de remisión que entregó el proveedor
        UiHelper.Etiqueta(this, "N° doc.:", 395, 20);
        _txtDocumento = UiHelper.Caja(this, 455, 16, 145);
        _txtDocumento.PlaceholderText = "F001-000123";
        _txtDocumento.CharacterCasing = CharacterCasing.Upper;
        _txtDocumento.MaxLength = 13;

        UiHelper.Etiqueta(this, "Producto:", 20, 60);
        _cboProducto = UiHelper.Combo(this, 100, 56, 280);
        _cboProducto.DisplayMember = nameof(Producto.Nombre); // solo el nombre
        _cboProducto.ValueMember = nameof(Producto.Id);       // el Id se conserva para la BD
        _cboProducto.SelectedIndexChanged += (_, _) => MostrarCosto();

        UiHelper.Etiqueta(this, "Cant.:", 395, 60);
        _nudCantidad = new NumericUpDown { Left = 440, Top = 56, Width = 60, Minimum = 1, Maximum = 10000, Value = 1 };
        Controls.Add(_nudCantidad);
        _lblCosto = UiHelper.Etiqueta(this, "Costo unit.: S/ 0.00", 515, 60);

        _lblFiltro = UiHelper.Etiqueta(this, "", 100, 82);
        _lblFiltro.Font = new Font("Segoe UI", 8.25F);
        _lblFiltro.ForeColor = Tema.Gris;

        var btnAgregar = UiHelper.Boton(this, "Agregar", 610, 14, 90, color: Tema.Secundario);
        btnAgregar.Click += BtnAgregar_Click;

        _grid = UiHelper.Grilla(this, 20, 105, 680, 275);
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
            _cargando = true;
            var seleccionado = (_cboProveedor.SelectedItem as Proveedor)?.Id;
            var lista = _proveedores.Listar();
            _cboProveedor.DataSource = lista;
            var previo = lista.FirstOrDefault(p => p.Id == seleccionado);
            if (previo != null) _cboProveedor.SelectedItem = previo; // conserva el proveedor elegido
        }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
        finally { _cargando = false; }
        CargarProductos();
    }

    // Al cambiar de proveedor: se muestran solo sus productos y se vacía la lista
    // (una factura/guía es de un solo proveedor)
    private void CambioProveedor()
    {
        if (_cargando) return;
        if (_detalle.Count > 0)
        {
            _detalle.Clear();
            ActualizarTotal();
            UiHelper.Error("Cambió el proveedor: se vació la lista porque una factura o guía es de un solo proveedor.");
        }
        CargarProductos();
    }

    private void CargarProductos()
    {
        try
        {
            var proveedor = _cboProveedor.SelectedItem as Proveedor;
            var productos = _compras.ProductosDelProveedor(proveedor);
            _cboProducto.DataSource = productos;
            _lblFiltro.Text = proveedor == null
                ? ""
                : productos.Count == 0
                    ? $"{proveedor.Empresa} no tiene productos asignados (asígnelos en Productos)."
                    : $"Mostrando solo los {productos.Count} producto(s) de {proveedor.Empresa}.";
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
            // El servicio valida el N° de documento, el proveedor de cada producto y vuelve a leer el costo de la BD
            var ingreso = _compras.Registrar(_cboProveedor.SelectedItem as Proveedor, _txtDocumento.Text, _detalle.ToList());
            MessageBox.Show($"Stock aumentado correctamente (ingreso N° {ingreso.Id}).\n" +
                            $"Proveedor: {ingreso.Proveedor?.Empresa}\n" +
                            $"Factura/guía: {ingreso.NumeroDocumento}\n" +
                            $"Unidades: {ingreso.Detalles.Sum(d => d.Cantidad)}\nCosto total: S/ {ingreso.Total:N2}",
                            "Aumentar stock", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _detalle.Clear();
            _txtDocumento.Clear();
            ActualizarTotal();
            CargarCombos();
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }
}
