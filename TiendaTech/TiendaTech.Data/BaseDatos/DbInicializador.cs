namespace TiendaTech.Data;

// Crea las tablas (si no existen) y carga datos de ejemplo la primera vez.
// El mismo script esta en la carpeta BaseDatos/TiendaTech.sql
public static class DbInicializador
{
    private const string Esquema = """
CREATE TABLE IF NOT EXISTS Usuario (
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    NombreUsuario TEXT    NOT NULL UNIQUE,
    Clave         TEXT    NOT NULL,
    Rol           TEXT    NOT NULL
);

CREATE TABLE IF NOT EXISTS Cliente (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre    TEXT    NOT NULL,
    Documento TEXT    NOT NULL UNIQUE,
    Telefono  TEXT
);

CREATE TABLE IF NOT EXISTS Proveedor (
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    Empresa  TEXT    NOT NULL,
    Ruc      TEXT    NOT NULL UNIQUE,
    Telefono TEXT
);

CREATE TABLE IF NOT EXISTS Producto (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre      TEXT          NOT NULL,
    Marca       TEXT,
    Precio      DECIMAL(10,2) NOT NULL CHECK (Precio > 0),
    Costo       DECIMAL(10,2) NOT NULL DEFAULT 0 CHECK (Costo >= 0),
    Stock       INTEGER       NOT NULL CHECK (Stock >= 0),
    ProveedorId INTEGER       REFERENCES Proveedor(Id)
);

CREATE TABLE IF NOT EXISTS Venta (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
    Fecha     TEXT          NOT NULL,
    ClienteId INTEGER       NOT NULL REFERENCES Cliente(Id),
    UsuarioId INTEGER       NOT NULL REFERENCES Usuario(Id),
    Subtotal  DECIMAL(10,2) NOT NULL,
    Descuento DECIMAL(10,2) NOT NULL,
    Total     DECIMAL(10,2) NOT NULL
);

CREATE TABLE IF NOT EXISTS DetalleVenta (
    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
    VentaId        INTEGER       NOT NULL REFERENCES Venta(Id),
    ProductoId     INTEGER       NOT NULL REFERENCES Producto(Id),
    Cantidad       INTEGER       NOT NULL CHECK (Cantidad > 0),
    PrecioUnitario DECIMAL(10,2) NOT NULL,
    Subtotal       DECIMAL(10,2) NOT NULL
);

CREATE TABLE IF NOT EXISTS Compra (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Fecha       TEXT          NOT NULL,
    ProveedorId INTEGER       NOT NULL REFERENCES Proveedor(Id),
    UsuarioId   INTEGER       NOT NULL REFERENCES Usuario(Id),
    Total       DECIMAL(10,2) NOT NULL
);

CREATE TABLE IF NOT EXISTS DetalleCompra (
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    CompraId      INTEGER       NOT NULL REFERENCES Compra(Id),
    ProductoId    INTEGER       NOT NULL REFERENCES Producto(Id),
    Cantidad      INTEGER       NOT NULL CHECK (Cantidad > 0),
    CostoUnitario DECIMAL(10,2) NOT NULL CHECK (CostoUnitario > 0),
    Subtotal      DECIMAL(10,2) NOT NULL
);
""";

    public static void Inicializar(IConexionFactory fabrica)
    {
        using var cn = fabrica.Abrir();

        foreach (var sentencia in Esquema.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            using var cmd = cn.Comando(sentencia);
            cmd.ExecuteNonQuery();
        }

        Migrar(cn);

        using (var cmd = cn.Comando("SELECT COUNT(*) FROM Usuario;"))
            if (Convert.ToInt32(cmd.ExecuteScalar()) > 0) return; // ya tiene datos

        using var tx = cn.BeginTransaction();
        foreach (var sentencia in DatosIniciales.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            using var cmd = cn.Comando(sentencia, tx);
            cmd.ExecuteNonQuery();
        }
        tx.Commit();
    }

    // Actualiza bases de datos creadas con una versión anterior SIN borrar información
    private static void Migrar(System.Data.Common.DbConnection cn)
    {
        // v2: columna Costo en Producto (costo predefinido para "Aumentar stock")
        using (var cmd = cn.Comando("SELECT COUNT(*) FROM pragma_table_info('Producto') WHERE name = 'Costo';"))
        {
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
            {
                using (var alter = cn.Comando("ALTER TABLE Producto ADD COLUMN Costo DECIMAL(10,2) NOT NULL DEFAULT 0;"))
                    alter.ExecuteNonQuery();

                // Toma como costo el último costo real pagado en una compra (si existe)
                using var upd = cn.Comando("""
                    UPDATE Producto
                    SET Costo = (SELECT d.CostoUnitario FROM DetalleCompra d
                                 WHERE d.ProductoId = Producto.Id ORDER BY d.Id DESC LIMIT 1)
                    WHERE EXISTS (SELECT 1 FROM DetalleCompra d WHERE d.ProductoId = Producto.Id);
                    """);
                upd.ExecuteNonQuery();
            }
        }
    }

    private const string DatosIniciales = """
INSERT INTO Usuario (NombreUsuario, Clave, Rol) VALUES
    ('admin', '1234', 'Administrador'),
    ('vendedor', '1234', 'Vendedor');

INSERT INTO Cliente (Nombre, Documento, Telefono) VALUES
    ('Cliente General', '00000000', '-'),
    ('María López', '45879632', '987654321'),
    ('Carlos Ramírez', '70214589', '912345678'),
    ('Lucía Torres', '46325871', '956123478');

INSERT INTO Proveedor (Empresa, Ruc, Telefono) VALUES
    ('TecnoImport SAC', '20123456789', '014445555'),
    ('Distribuidora Andina Tech SAC', '20556677881', '017203040'),
    ('Accesorios Lima EIRL', '20601234567', '993456120');

INSERT INTO Producto (Nombre, Marca, Precio, Costo, Stock, ProveedorId) VALUES
    ('Celular Galaxy A54', 'Samsung', 1200, 950, 9, 1),
    ('Laptop Aspire 5', 'Acer', 2500, 1950, 5, 2),
    ('Audífonos Tune 520BT', 'JBL', 150, 95, 28, 3),
    ('Cargador 20W USB-C', 'Apple', 80, 55, 49, 3),
    ('Monitor 24" 24MP400', 'LG', 520, 380, 8, 2),
    ('Mouse inalámbrico M185', 'Logitech', 45, 28, 40, 3),
    ('Teclado mecánico K552', 'Redragon', 160, 110, 15, 3),
    ('Smartwatch Redmi Watch 3', 'Xiaomi', 280, 200, 12, 1);

INSERT INTO Venta (Fecha, ClienteId, UsuarioId, Subtotal, Descuento, Total) VALUES
    ('2026-10-15 10:24:00', 2, 2, 1280, 64, 1216),
    ('2026-10-16 17:05:00', 3, 2, 300, 0, 300);

INSERT INTO DetalleVenta (VentaId, ProductoId, Cantidad, PrecioUnitario, Subtotal) VALUES
    (1, 1, 1, 1200, 1200),
    (1, 4, 1, 80, 80),
    (2, 3, 2, 150, 300);

INSERT INTO Compra (Fecha, ProveedorId, UsuarioId, Total) VALUES
    ('2026-10-14 09:00:00', 3, 1, 1510);

INSERT INTO DetalleCompra (CompraId, ProductoId, Cantidad, CostoUnitario, Subtotal) VALUES
    (1, 6, 20, 28, 560),
    (1, 3, 10, 95, 950)
""";
}
