using TiendaTech.Business;
using TiendaTech.Models;

namespace TiendaTech.UI;

// PANTALLA 3 - Clientes (tabla Cliente)
public class FrmClientes : FormBase
{
    private readonly IClienteService _service;
    private readonly TextBox _txtNombre, _txtDocumento, _txtTelefono;
    private readonly DataGridView _grid;

    public FrmClientes(IClienteService service)
    {
        _service = service;
        Text = "Clientes";
        ClientSize = new Size(640, 420);
        StartPosition = FormStartPosition.CenterScreen;

        UiHelper.Etiqueta(this, "Nombre:", 20, 20);
        _txtNombre = UiHelper.Caja(this, 100, 16, 200);
        UiHelper.Etiqueta(this, "DNI / RUC:", 20, 55);
        _txtDocumento = UiHelper.Caja(this, 100, 51, 200);
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
        UiHelper.Columna(_grid, "Nombre", "Nombre", peso: 150);
        UiHelper.Columna(_grid, "Documento", "DNI / RUC");
        UiHelper.Columna(_grid, "Telefono", "Teléfono");
        Cargar();
    }

    private void Cargar() => _grid.DataSource = _service.Listar();

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        try
        {
            _service.Registrar(new Cliente
            {
                Nombre = _txtNombre.Text.Trim(),
                Documento = _txtDocumento.Text.Trim(),
                Telefono = _txtTelefono.Text
            });
            _txtNombre.Clear(); _txtDocumento.Clear(); _txtTelefono.Clear();
            Cargar();
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }

    private void BtnEliminar_Click(object? sender, EventArgs e)
    {
        if (_grid.CurrentRow?.DataBoundItem is not Cliente x) { UiHelper.Error("Seleccione una fila de la tabla."); return; }
        if (!UiHelper.Confirmar($"¿Seguro que desea eliminar {x.Nombre}?")) return;
        try
        {
            _service.Eliminar(x);
            Cargar();
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }
}
