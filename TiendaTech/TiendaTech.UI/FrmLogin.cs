using Microsoft.Extensions.DependencyInjection;
using TiendaTech.Business;

namespace TiendaTech.UI;

// PANTALLA 1 - Login (valida contra la tabla Usuario)
public class FrmLogin : FormBase
{
    private readonly IAuthService _auth;
    private readonly IServiceProvider _provider;
    private readonly TextBox _txtUsuario;
    private readonly TextBox _txtClave;
    private readonly Label _lblAviso;

    public FrmLogin(IAuthService auth, IServiceProvider provider)
    {
        _auth = auth;
        _provider = provider;

        Text = "EvoTech - Iniciar sesión";
        ClientSize = new Size(320, 230);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        UiHelper.Etiqueta(this, "Iniciar sesión", 70, 15, true);
        UiHelper.Etiqueta(this, "Usuario:", 30, 70);
        _txtUsuario = UiHelper.Caja(this, 110, 66, 170);
        UiHelper.Etiqueta(this, "Clave:", 30, 110);
        _txtClave = UiHelper.Caja(this, 110, 106, 170, clave: true);
        _lblAviso = UiHelper.Aviso(this, 30, 134);
        // Sin mayúsculas ni espacios: se bloquean mientras se escribe o se pega
        const string msg = "Sin mayúsculas ni espacios";
        UiHelper.Filtrar(_txtUsuario, Validaciones.CaracterPermitidoCredencial, _lblAviso, msg);
        UiHelper.Filtrar(_txtClave, Validaciones.CaracterPermitidoCredencial, _lblAviso, msg);
        var btn = UiHelper.Boton(this, "Ingresar", 110, 155, 170);
        btn.Click += BtnIngresar_Click;
        AcceptButton = btn;
        UiHelper.Etiqueta(this, "Demo: admin / 1234", 110, 195);
    }

    private void BtnIngresar_Click(object? sender, EventArgs e)
    {
        try
        {
            // Se envía el texto tal cual: el servicio vuelve a validar (no se puede saltar la regla)
            var usuario = _auth.Validar(_txtUsuario.Text, _txtClave.Text);
            if (usuario == null)
            {
                UiHelper.Error("Usuario o clave incorrectos.");
                return;
            }
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); return; }
        catch (Exception ex) { UiHelper.ErrorBd(ex); return; }

        var menu = _provider.GetRequiredService<FrmMenuPrincipal>();
        menu.FormClosed += (_, _) =>
        {
            if (menu.CerroSesion)
            {
                _txtUsuario.Clear();
                _txtClave.Clear();
                _lblAviso.Visible = false;
                Show();               // vuelve a mostrar el login
                _txtUsuario.Focus();
            }
            else Application.Exit();  // cerró el menú con la X: sale del programa
        };
        menu.Show();
        Hide();
    }
}
