using TiendaTech.Business;

namespace TiendaTech.UI;

// Cambiar la clave del usuario que tiene la sesión abierta (la regla vive en UsuarioService)
public class FrmCambiarClave : FormBase
{
    private readonly IUsuarioService _service;
    private readonly TextBox _txtActual, _txtNueva, _txtConfirmar;

    public FrmCambiarClave(IUsuarioService service)
    {
        _service = service;
        Text = "Cambiar clave";
        ClientSize = new Size(360, 260);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        UiHelper.Etiqueta(this, "Cambiar clave", 100, 15, true);
        UiHelper.Etiqueta(this, "Clave actual:", 20, 65);
        _txtActual = UiHelper.Caja(this, 150, 61, 190, clave: true);
        UiHelper.Etiqueta(this, "Clave nueva:", 20, 100);
        _txtNueva = UiHelper.Caja(this, 150, 96, 190, clave: true);
        UiHelper.Etiqueta(this, "Repetir nueva:", 20, 135);
        _txtConfirmar = UiHelper.Caja(this, 150, 131, 190, clave: true);

        var aviso = UiHelper.Aviso(this, 20, 160);
        const string msg = "Sin mayúsculas ni espacios";
        UiHelper.Filtrar(_txtActual, Validaciones.CaracterPermitidoCredencial, aviso, msg);
        UiHelper.Filtrar(_txtNueva, Validaciones.CaracterPermitidoCredencial, aviso, msg);
        UiHelper.Filtrar(_txtConfirmar, Validaciones.CaracterPermitidoCredencial, aviso, msg);

        var btnGuardar = UiHelper.Boton(this, "Guardar", 150, 195, 90);
        btnGuardar.Click += BtnGuardar_Click;
        var btnCancelar = UiHelper.Boton(this, "Cancelar", 250, 195, 90, color: Tema.Gris);
        btnCancelar.Click += (_, _) => Close();
        AcceptButton = btnGuardar;
        CancelButton = btnCancelar;
    }

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        try
        {
            _service.CambiarClave(_txtActual.Text, _txtNueva.Text, _txtConfirmar.Text);
            MessageBox.Show("La clave se cambió correctamente.", "EvoTech",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (UnauthorizedAccessException ex) { UiHelper.Error(ex.Message); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }
}
