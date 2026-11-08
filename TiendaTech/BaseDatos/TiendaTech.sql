-- =============================================================
--  TiendaTech - Script de base de datos (SQLite)
--  El programa ejecuta este mismo script automaticamente la
--  primera vez que se abre (clase DbInicializador).
-- =============================================================

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
    Total       DECIMAL(10,2) NOT NULL,
    NumeroDocumento TEXT      -- N° de factura/guía del proveedor (v3, ver EsquemaCompras.cs)
);

-- Un proveedor no puede tener dos ingresos con el mismo N° de factura/guía
CREATE UNIQUE INDEX IF NOT EXISTS UX_Compra_Proveedor_Documento
    ON Compra (ProveedorId, NumeroDocumento) WHERE NumeroDocumento IS NOT NULL;

CREATE TABLE IF NOT EXISTS DetalleCompra (
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    CompraId      INTEGER       NOT NULL REFERENCES Compra(Id),
    ProductoId    INTEGER       NOT NULL REFERENCES Producto(Id),
    Cantidad      INTEGER       NOT NULL CHECK (Cantidad > 0),
    CostoUnitario DECIMAL(10,2) NOT NULL CHECK (CostoUnitario > 0),
    Subtotal      DECIMAL(10,2) NOT NULL
);
