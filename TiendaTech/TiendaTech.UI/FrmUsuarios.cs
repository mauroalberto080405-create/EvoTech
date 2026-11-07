using TiendaTech.Business;
using TiendaTech.Models;

namespace TiendaTech.UI;

// PANTALLA 4 - Usuarios del sistema (tabla Usuario, solo Administrador)
public class FrmUsuarios : FormBase
{
    private readonly IUsuarioService _service;
    private readonly TextBox _txtUsuario, _txtClave;
    private readonly ComboBox _cboRol;
    private readonly DataGridView _grid;

    public FrmUsuarios(IUsuarioService service)
    {
        _service = service;
        Text = "Usuarios";
        ClientSize = new Size(640, 420);
        StartPosition = FormStartPosition.CenterScreen;

        UiHelper.Etiqueta(this, "Usuario:", 20, 20);
        _txtUsuario = UiHelper.Caja(this, 100, 16, 200);
        UiHelper.Etiqueta(this, "Clave:", 20, 55);
        _txtClave = UiHelper.Caja(this, 100, 51, 200, clave: true);
        UiHelper.Etiqueta(this, "Rol:", 20, 90);
        _cboRol = UiHelper.Combo(this, 100, 86, 200);
        var aviso = UiHelper.Aviso(this, 100, 112);
        UiHelper.Filtrar(_txtUsuario, Validaciones.CaracterPermitidoCredencial, aviso, "Sin mayúsculas ni espacios");
        UiHelper.Filtrar(_txtClave, Validaciones.CaracterPermitidoCredencial, aviso, "Sin mayúsculas ni espacios");
        _cboRol.Items.AddRange(UsuarioService.Roles);
        _cboRol.SelectedIndex = 0;
        var btn = UiHelper.Boton(this, "Guardar", 340, 50, 120);
        btn.Click += BtnGuardar_Click;
        var btnEliminar = UiHelper.Boton(this, "Eliminar", 480, 50, 120, color: Tema.Peligro);
        btnEliminar.Click += BtnEliminar_Click;

        _grid = UiHelper.Grilla(this, 20, 135, 600, 265);
        UiHelper.Columna(_grid, "Id", "Id", peso: 30);
        UiHelper.Columna(_grid, "NombreUsuario", "Usuario", peso: 150);
        UiHelper.Columna(_grid, "Rol", "Rol", peso: 120);
        Cargar();
    }

    private void Cargar() => _grid.DataSource = _service.Listar(); // la clave no se muestra

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        try
        {
            _service.Registrar(new Usuario
            {
                NombreUsuario = _txtUsuario.Text,
                Clave = _txtClave.Text,
                Rol = _cboRol.SelectedItem as string ?? ""
            });
            _txtUsuario.Clear(); _txtClave.Clear(); _cboRol.SelectedIndex = 0;
            Cargar();
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }

    private void BtnEliminar_Click(object? sender, EventArgs e)
    {
        if (_grid.CurrentRow?.DataBoundItem is not Usuario x) { UiHelper.Error("Seleccione una fila de la tabla."); return; }
        if (!UiHelper.Confirmar($"¿Seguro que desea eliminar {x.NombreUsuario}?")) return;
        try
        {
            _service.Eliminar(x);
            Cargar();
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }
}
