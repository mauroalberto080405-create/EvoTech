namespace TiendaTech.Data;

// Asegura que la tabla Producto tenga la columna StockMinimo (mínimo propio de cada producto, T3).
// - BD nueva o antigua: si falta la columna la agrega SIN borrar datos (los productos existentes quedan con mínimo 5).
// Está separado de DbInicializador para que esta mejora no toque archivos de otros módulos.
public static class EsquemaProductos
{
    public const int MinimoPorDefecto = 5;

    public static void Asegurar(IConexionFactory fabrica)
    {
        using var cn = fabrica.Abrir();
        using var cmd = cn.Comando("SELECT COUNT(*) FROM pragma_table_info('Producto') WHERE name = 'StockMinimo';");
        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0) return;

        using var alter = cn.Comando($"ALTER TABLE Producto ADD COLUMN StockMinimo INTEGER NOT NULL DEFAULT {MinimoPorDefecto};");
        alter.ExecuteNonQuery();
    }
}
