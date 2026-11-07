using Microsoft.Extensions.DependencyInjection;
using TiendaTech.Business;
using TiendaTech.Data;

namespace TiendaTech.UI;

// PANTALLA 2 - Menu principal (centro de navegacion)
public class FrmMenuPrincipal : FormBase
{
    private readonly IServiceProvider _provider;
    private readonly ISesion _sesion;

    // El login lo revisa al cerrarse el menú: true = volver al login, false = salir del programa
    public bool CerroSesion { get; private set; }

    public FrmMenuPrincipal(IServiceProvider provider, ISesion sesion, IConexionFactory bd)
    {
        _provider = provider;
        _sesion = sesion;
        var usuario = sesion.UsuarioActual;
        bool esAdmin = usuario?.Rol == "Administrador";

        Text = "EvoTech - Menú principal";
        ClientSize = new Size(520, 430);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        UiHelper.Etiqueta(this, "EvoTech", 200, 15, true);
        UiHelper.Etiqueta(this, $"Bienvenido, {usuario?.NombreUsuario} ({usuario?.Rol})", 30, 60);

        var lblVentas = UiHelper.Etiqueta(this, "VENTAS", 30, 95);
        var lblInventario = UiHelper.Etiqueta(this, "INVENTARIO Y COMPRAS", 270, 95);
        lblVentas.Font = lblInventario.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        lblVentas.ForeColor = Tema.Primario;
        lblInventario.ForeColor = Tema.Secundario;

        // Columna izquierda: ventas y administracion
        Opcion<FrmNuevaVenta>("Nueva venta", 30, 120);
        Opcion<FrmHistorialVentas>("Historial de ventas", 30, 170);
        Opcion<FrmReporteVentas>("Reporte de ventas", 30, 220);
        Opcion<FrmClientes>("Clientes", 30, 270);
        Opcion<FrmUsuarios>("Usuarios", 30, 320).Enabled = esAdmin;

        // Columna derecha: inventario y compras
        Opcion<FrmProductos>("Productos", 270, 120);
        Opcion<FrmStockBajo>("Alertas de stock bajo", 270, 170);
        Opcion<FrmProveedores>("Proveedores", 270, 220).Enabled = esAdmin;
        Opcion<FrmCompras>("Aumentar stock", 270, 270).Enabled = esAdmin;
        Opcion<FrmHistorialCompras>("Historial de compras", 270, 320);

        var btnCerrar = UiHelper.Boton(this, "Cerrar sesión", 390, 18, 110, 30, color: Tema.Gris);
        btnCerrar.Click += BtnCerrarSesion_Click;

        var lblBd = UiHelper.Etiqueta(this, $"● Base de datos conectada: {bd.Descripcion}", 30, 395);
        lblBd.ForeColor = Color.ForestGreen;
    }

    private void BtnCerrarSesion_Click(object? sender, EventArgs e)
    {
        if (!UiHelper.Confirmar("¿Desea cerrar sesión?")) return;

        _sesion.Cerrar();
        CerroSesion = true;

        // Cierra las demás ventanas abiertas (clientes, ventas, etc.) menos el login
        foreach (var f in Application.OpenForms.Cast<Form>().ToList())
            if (f != this && f is not FrmLogin) f.Close();

        Close();
    }

    // El menu no conoce la logica de las ventanas: solo las pide al contenedor de dependencias
    private Button Opcion<T>(string texto, int x, int y) where T : Form
    {
        var b = UiHelper.Boton(this, texto, x, y, 220, 40, x < 200 ? Tema.Primario : Tema.Secundario);
        b.Click += (_, _) => _provider.GetRequiredService<T>().Show();
        return b;
    }
}
