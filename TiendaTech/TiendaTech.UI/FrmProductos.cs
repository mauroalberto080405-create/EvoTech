using TiendaTech.Business;
using TiendaTech.Models;

namespace TiendaTech.UI;

// PANTALLA 5 - Productos / inventario (tabla Producto, relacionada con Proveedor)
// Modo NUEVO: se llena el formulario y se presiona Guardar.
// Modo EDICIÓN: clic en una fila -> se cargan sus datos -> se cambia -> Actualizar.
public class FrmProductos : FormBase
{
    private readonly IProductoService _service;
    private readonly TextBox _txtNombre, _txtMarca, _txtBuscar;
    private readonly NumericUpDown _nudPrecio, _nudCosto, _nudStock, _nudMinimo;
    private readonly ComboBox _cboProveedor;
    private readonly Button _btnGuardar, _btnActualizar;
    private readonly Label _lblModo;
    private readonly DataGridView _grid;
    private int _idEditando; // 0 = producto nuevo

    public FrmProductos(IProductoService service, IProveedorService proveedores)
    {
        _service = service;
        Text = "Productos";
        ClientSize = new Size(760, 510);
        StartPosition = FormStartPosition.CenterScreen;

        UiHelper.Etiqueta(this, "Nombre:", 20, 20);
        _txtNombre = UiHelper.Caja(this, 95, 16, 210);
        UiHelper.Etiqueta(this, "Marca:", 20, 55);
        _txtMarca = UiHelper.Caja(this, 95, 51, 210);
        UiHelper.Etiqueta(this, "Proveedor:", 20, 90);
        _cboProveedor = UiHelper.Combo(this, 95, 86, 210);
        _cboProveedor.DataSource = proveedores.Listar();

        UiHelper.Etiqueta(this, "Precio S/:", 325, 20);
        _nudPrecio = new NumericUpDown { Left = 400, Top = 16, Width = 120, DecimalPlaces = 2, Maximum = 1000000 };
        Controls.Add(_nudPrecio);
        UiHelper.Etiqueta(this, "Costo S/:", 325, 55);
        _nudCosto = new NumericUpDown { Left = 400, Top = 51, Width = 120, DecimalPlaces = 2, Maximum = 1000000 };
        Controls.Add(_nudCosto);
        UiHelper.Etiqueta(this, "Stock:", 325, 90);
        _nudStock = new NumericUpDown { Left = 400, Top = 86, Width = 120, Maximum = 100000 };
        Controls.Add(_nudStock);
        UiHelper.Etiqueta(this, "Stock mín.:", 325, 125);
        _nudMinimo = new NumericUpDown { Left = 400, Top = 121, Width = 120, Maximum = 100000, Value = 5 };
        Controls.Add(_nudMinimo);

        UiHelper.Etiqueta(this, "Buscar:", 20, 125);
        _txtBuscar = UiHelper.Caja(this, 95, 121, 210);
        _txtBuscar.PlaceholderText = "Nombre, marca o proveedor";
        _txtBuscar.TextChanged += (_, _) => Cargar();

        _btnGuardar = UiHelper.Boton(this, "Guardar", 540, 14, 100, 32);
        _btnGuardar.Click += BtnGuardar_Click;
        _btnActualizar = UiHelper.Boton(this, "Actualizar", 650, 14, 100, 32, color: Tema.Secundario);
        _btnActualizar.Click += BtnActualizar_Click;
        var btnLimpiar = UiHelper.Boton(this, "Limpiar", 540, 52, 100, 32, color: Tema.Gris);
        btnLimpiar.Click += (_, _) => Limpiar();
        var btnEliminar = UiHelper.Boton(this, "Eliminar", 650, 52, 100, 32, color: Tema.Peligro);
        btnEliminar.Click += BtnEliminar_Click;
        _lblModo = UiHelper.Etiqueta(this, "", 540, 92);
        _lblModo.Font = new Font("Segoe UI", 8.25F);
        _lblModo.ForeColor = Tema.Gris;

        _grid = UiHelper.Grilla(this, 20, 160, 720, 330);
        UiHelper.Columna(_grid, "Id", "Id", peso: 30);
        UiHelper.Columna(_grid, "Nombre", "Producto", peso: 160);
        UiHelper.Columna(_grid, "Marca", "Marca", peso: 80);
        UiHelper.Columna(_grid, "Precio", "Precio S/", "N2", 65);
        UiHelper.Columna(_grid, "Costo", "Costo S/", "N2", 65);
        UiHelper.Columna(_grid, "Stock", "Stock", peso: 45);
        UiHelper.Columna(_grid, "StockMinimo", "Mínimo", peso: 45);
        _grid.CellFormatting += PintarStockBajo;
        _grid.CellClick += (_, e) => { if (e.RowIndex >= 0) CargarEnFormulario(); };

        Cargar();
        Limpiar();
    }

    private void Cargar() => _grid.DataSource = _service.Buscar(_txtBuscar.Text);

    // Los productos con stock igual o menor a su mínimo se pintan en rojo
    private void PintarStockBajo(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex >= 0 && _grid.Rows[e.RowIndex].DataBoundItem is Producto p && p.EsStockBajo)
            e.CellStyle.ForeColor = Color.Firebrick;
    }

    // Pasa los datos de la fila seleccionada al formulario para editarlos
    private void CargarEnFormulario()
    {
        if (_grid.CurrentRow?.DataBoundItem is not Producto p) return;
        _idEditando = p.Id;
        _txtNombre.Text = p.Nombre;
        _txtMarca.Text = p.Marca;
        _nudPrecio.Value = Math.Min(p.Precio, _nudPrecio.Maximum);
        _nudCosto.Value = Math.Min(p.Costo, _nudCosto.Maximum);
        _nudStock.Value = Math.Min(p.Stock, _nudStock.Maximum);
        _nudMinimo.Value = Math.Min(p.StockMinimo, _nudMinimo.Maximum);
        _cboProveedor.SelectedItem = (_cboProveedor.DataSource as List<Proveedor>)?.FirstOrDefault(x => x.Id == p.ProveedorId);
        if (p.ProveedorId == null) _cboProveedor.SelectedIndex = -1;
        ModoEdicion(true, p.Nombre);
    }

    private void Limpiar()
    {
        _idEditando = 0;
        _txtNombre.Clear(); _txtMarca.Clear();
        _nudPrecio.Value = 0; _nudCosto.Value = 0; _nudStock.Value = 0; _nudMinimo.Value = 5;
        if (_cboProveedor.Items.Count > 0) _cboProveedor.SelectedIndex = 0;
        ModoEdicion(false, "");
    }

    // En edición: no se puede "Guardar" (evita crear un duplicado) y el stock queda bloqueado,
    // porque el stock solo cambia con ventas y con "Aumentar stock".
    private void ModoEdicion(bool editando, string nombre)
    {
        _btnGuardar.Enabled = !editando;
        _btnActualizar.Enabled = editando;
        _nudStock.Enabled = !editando;
        _lblModo.Text = editando ? $"Editando: {nombre}" : "Clic en una fila para editar";
    }

    private Producto LeerFormulario()
    {
        var proveedor = _cboProveedor.SelectedItem as Proveedor;
        return new Producto
        {
            Id = _idEditando,
            Nombre = _txtNombre.Text,
            Marca = _txtMarca.Text,
            Precio = _nudPrecio.Value,
            Costo = _nudCosto.Value,
            Stock = (int)_nudStock.Value,
            StockMinimo = (int)_nudMinimo.Value,
            ProveedorId = proveedor?.Id,
            ProveedorNombre = proveedor?.Empresa ?? ""
        };
    }

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        try
        {
            var nuevo = LeerFormulario();
            nuevo.Id = 0;
            _service.Registrar(nuevo);
            Cargar();
            Limpiar();
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }

    private void BtnActualizar_Click(object? sender, EventArgs e)
    {
        if (_idEditando == 0) { UiHelper.Error("Haga clic en el producto de la tabla que desea actualizar."); return; }
        try
        {
            var p = LeerFormulario();
            _service.Actualizar(p);
            Cargar();
            Limpiar();
            MessageBox.Show($"Producto \"{p.Nombre}\" actualizado correctamente.", "Producto actualizado",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }

    private void BtnEliminar_Click(object? sender, EventArgs e)
    {
        if (_grid.CurrentRow?.DataBoundItem is not Producto x) { UiHelper.Error("Seleccione una fila de la tabla."); return; }
        if (!UiHelper.Confirmar($"¿Seguro que desea eliminar {x.Nombre}?")) return;
        try
        {
            _service.Eliminar(x);
            Cargar();
            Limpiar();
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }
}
