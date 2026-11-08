using Microsoft.Extensions.DependencyInjection;
using TiendaTech.Business;
using TiendaTech.Data;

namespace TiendaTech.UI;

static class Program
{
    // Composition root: el UNICO lugar donde se eligen las clases concretas (DIP)
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // 1. Base de datos: archivo EvoTech.db junto al .exe (se crea solo la primera vez)
        var rutaBd = Path.Combine(AppContext.BaseDirectory, "EvoTech.db");
        IConexionFactory fabrica = new SqliteConexionFactory(rutaBd);
        try
        {
            DbInicializador.Inicializar(fabrica);
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo conectar a la base de datos:\n" + ex.Message, "EvoTech",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var services = new ServiceCollection();

        // 2. Data: repositorios con base de datos.
        //    DIP: cambiar "UsuarioSqlRepository" por "UsuarioMemoryRepository" (y asi con los demas)
        //    hace que el sistema funcione sin BD, sin tocar servicios ni pantallas.
        services.AddSingleton(fabrica);
        services.AddSingleton<IUsuarioRepository, UsuarioSqlRepository>();
        services.AddSingleton<IClienteRepository, ClienteSqlRepository>();
        services.AddSingleton<IProductoRepository, ProductoSqlRepository>();
        services.AddSingleton<IProveedorRepository, ProveedorSqlRepository>();
        services.AddSingleton<IVentaRepository, VentaSqlRepository>();
        services.AddSingleton<ICompraRepository, CompraSqlRepository>();
        services.AddSingleton<IReporteRepository, ReporteSqlRepository>();

        // 3. Business: servicios y regla de descuento
        //    OCP: cambiar por SinDescuento, o por: services.AddSingleton<IDescuento>(new DescuentoCampania(0.10m));
        services.AddSingleton<IDescuento, DescuentoPorMonto>();
        services.AddSingleton<IHasheadorClave, HasheadorPbkdf2>(); // claves guardadas con hash
        services.AddSingleton<ISesion, Sesion>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IClienteService, ClienteService>();
        services.AddTransient<IProductoService, ProductoService>();
        services.AddTransient<IProveedorService, ProveedorService>();
        services.AddTransient<IVentaService, VentaService>();
        services.AddTransient<ICompraService, CompraService>();
        services.AddTransient<IUsuarioService, UsuarioService>();
        services.AddTransient<IReporteService, ReporteService>();

        // 4. UI: las 12 pantallas
        services.AddTransient<FrmLogin>();            // 1
        services.AddTransient<FrmMenuPrincipal>();    // 2
        services.AddTransient<FrmClientes>();         // 3
        services.AddTransient<FrmUsuarios>();         // 4
        services.AddTransient<FrmProductos>();        // 5
        services.AddTransient<FrmStockBajo>();        // 6
        services.AddTransient<FrmProveedores>();      // 7
        services.AddTransient<FrmCompras>();          // 8
        services.AddTransient<FrmNuevaVenta>();       // 9
        services.AddTransient<FrmHistorialVentas>();  // 10
        services.AddTransient<FrmHistorialCompras>(); // 11
        services.AddTransient<FrmReporteVentas>();    // 12

        var provider = services.BuildServiceProvider();
        Application.Run(provider.GetRequiredService<FrmLogin>());
    }
}
