using TiendaTech.Business;
using TiendaTech.Models;

namespace TiendaTech.UI;

// PANTALLA 7 - Proveedores (tabla Proveedor)
// Modo NUEVO: se llena el formulario y se presiona Guardar.
// Modo EDICIÓN: clic en una fila -> se cargan sus datos -> se cambia empresa o teléfono -> Actualizar.
public class FrmProveedores : FormBase
{
    private readonly IProveedorService _service;
    private readonly TextBox _txtEmpresa, _txtRuc, _txtTelefono;
    private readonly Button _btnGuardar, _btnActualizar;
    private readonly Label _lblModo;
    private readonly DataGridView _grid;
    private int _idEditando; // 0 = proveedor nuevo

    public FrmProveedores(IProveedorService service)
    {
        _service = service;
        Text = "Proveedores";
        ClientSize = new Size(640, 440);
        StartPosition = FormStartPosition.CenterScreen;

        UiHelper.Etiqueta(this, "Empresa:", 20, 20);
        _txtEmpresa = UiHelper.Caja(this, 100, 16, 200);
        UiHelper.Etiqueta(this, "RUC:", 20, 55);
        _txtRuc = UiHelper.Caja(this, 100, 51, 200);
        _txtRuc.MaxLength = 11;
        UiHelper.Etiqueta(this, "Teléfono:", 20, 90);
        _txtTelefono = UiHelper.Caja(this, 100, 86, 200);
        _txtTelefono.MaxLength = Validaciones.MaxTelefono;   // máximo 9 dígitos
        var avisoTel = UiHelper.Aviso(this, 100, 112);
        UiHelper.Filtrar(_txtTelefono, Validaciones.CaracterPermitidoTelefono, avisoTel, "Solo números (máx. 9 dígitos)");

        _btnGuardar = UiHelper.Boton(this, "Guardar", 340, 14, 120);
        _btnGuardar.Click += BtnGuardar_Click;
        _btnActualizar = UiHelper.Boton(this, "Actualizar", 480, 14, 120, color: Tema.Secundario);
        _btnActualizar.Click += BtnActualizar_Click;
        var btnLimpiar = UiHelper.Boton(this, "Limpiar", 340, 52, 120, color: Tema.Gris);
        btnLimpiar.Click += (_, _) => Limpiar();
        var btnEliminar = UiHelper.Boton(this, "Eliminar", 480, 52, 120, color: Tema.Peligro);
        btnEliminar.Click += BtnEliminar_Click;
        _lblModo = UiHelper.Etiqueta(this, "", 340, 92);
        _lblModo.Font = new Font("Segoe UI", 8.25F);
        _lblModo.ForeColor = Tema.Gris;

        _grid = UiHelper.Grilla(this, 20, 140, 600, 280);
        UiHelper.Columna(_grid, "Id", "Id", peso: 30);
        UiHelper.Columna(_grid, "Empresa", "Empresa", peso: 160);
        UiHelper.Columna(_grid, "Ruc", "RUC");
        UiHelper.Columna(_grid, "Telefono", "Teléfono");
        _grid.CellClick += (_, e) => { if (e.RowIndex >= 0) CargarEnFormulario(); };

        Load += (_, _) => { Cargar(); Limpiar(); };
    }

    private void Cargar()
    {
        try { _grid.DataSource = _service.Listar(); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }

    // Pasa los datos de la fila seleccionada al formulario para editarlos
    private void CargarEnFormulario()
    {
        if (_grid.CurrentRow?.DataBoundItem is not Proveedor p) return;
        _idEditando = p.Id;
        _txtEmpresa.Text = p.Empresa;
        _txtRuc.Text = p.Ruc;
        _txtTelefono.Text = p.Telefono;
        ModoEdicion(true, p.Empresa);
    }

    private void Limpiar()
    {
        _idEditando = 0;
        _txtEmpresa.Clear(); _txtRuc.Clear(); _txtTelefono.Clear();
        ModoEdicion(false, "");
    }

    // En edición no se puede "Guardar" (evita duplicar) y el RUC queda bloqueado: identifica al proveedor
    private void ModoEdicion(bool editando, string empresa)
    {
        _btnGuardar.Enabled = !editando;
        _btnActualizar.Enabled = editando;
        _txtRuc.ReadOnly = editando;
        _lblModo.Text = editando ? $"Editando: {empresa}" : "Clic en una fila para editar";
    }

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        try
        {
            _service.Registrar(new Proveedor
            {
                Empresa = _txtEmpresa.Text,   // el servicio quita espacios y exige mínimo 2 caracteres
                Ruc = _txtRuc.Text.Trim(),
                Telefono = _txtTelefono.Text
            });
            Cargar();
            Limpiar();
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }

    private void BtnActualizar_Click(object? sender, EventArgs e)
    {
        if (_idEditando == 0) { UiHelper.Error("Haga clic en el proveedor de la tabla que desea actualizar."); return; }
        try
        {
            var p = new Proveedor { Id = _idEditando, Empresa = _txtEmpresa.Text, Ruc = _txtRuc.Text, Telefono = _txtTelefono.Text };
            _service.Actualizar(p);
            Cargar();
            Limpiar();
            MessageBox.Show($"Proveedor \"{p.Empresa}\" actualizado correctamente.", "Proveedor actualizado",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }

    private void BtnEliminar_Click(object? sender, EventArgs e)
    {
        if (_grid.CurrentRow?.DataBoundItem is not Proveedor x) { UiHelper.Error("Seleccione una fila de la tabla."); return; }
        if (!UiHelper.Confirmar($"¿Seguro que desea eliminar {x.Empresa}?")) return;
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
