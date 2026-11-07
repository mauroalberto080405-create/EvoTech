using TiendaTech.Business;
using TiendaTech.Models;

namespace TiendaTech.UI;

// PANTALLA 7 - Proveedores (tabla Proveedor)
public class FrmProveedores : FormBase
{
    private readonly IProveedorService _service;
    private readonly TextBox _txtEmpresa, _txtRuc, _txtTelefono;
    private readonly DataGridView _grid;

    public FrmProveedores(IProveedorService service)
    {
        _service = service;
        Text = "Proveedores";
        ClientSize = new Size(640, 420);
        StartPosition = FormStartPosition.CenterScreen;

        UiHelper.Etiqueta(this, "Empresa:", 20, 20);
        _txtEmpresa = UiHelper.Caja(this, 100, 16, 200);
        UiHelper.Etiqueta(this, "RUC:", 20, 55);
        _txtRuc = UiHelper.Caja(this, 100, 51, 200);
        UiHelper.Etiqueta(this, "Teléfono:", 20, 90);
        _txtTelefono = UiHelper.Caja(this, 100, 86, 200);
        _txtTelefono.MaxLength = Validaciones.MaxTelefono;   // máximo 9 dígitos
        var avisoTel = UiHelper.Aviso(this, 100, 112);
        UiHelper.Filtrar(_txtTelefono, Validaciones.CaracterPermitidoTelefono, avisoTel, "Solo números (máx. 9 dígitos)");
        var btn = UiHelper.Boton(this, "Guardar", 340, 50, 120);
        btn.Click += BtnGuardar_Click;
        var btnEliminar = UiHelper.Boton(this, "Eliminar", 480, 50, 120, color: Tema.Peligro);
        btnEliminar.Click += BtnEliminar_Click;

        _grid = UiHelper.Grilla(this, 20, 135, 600, 265);
        UiHelper.Columna(_grid, "Id", "Id", peso: 30);
        UiHelper.Columna(_grid, "Empresa", "Empresa", peso: 160);
        UiHelper.Columna(_grid, "Ruc", "RUC");
        UiHelper.Columna(_grid, "Telefono", "Teléfono");
        Cargar();
    }

    private void Cargar() => _grid.DataSource = _service.Listar();

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
            _txtEmpresa.Clear(); _txtRuc.Clear(); _txtTelefono.Clear();
            Cargar();
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
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }
}
