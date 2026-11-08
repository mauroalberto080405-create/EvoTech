namespace TiendaTech.Data;

// Asegura que la tabla Venta tenga las columnas para anular ventas:
//   Anulada (0 = activa, 1 = anulada) y FechaAnulacion.
// - BD nueva o antigua: si faltan las columnas, las agrega SIN borrar datos (las ventas antiguas quedan activas).
// Está separado de DbInicializador para que esta mejora no toque archivos de otros módulos.
public static class EsquemaVentas
{
    public static void Asegurar(IConexionFactory fabrica)
    {
        using var cn = fabrica.Abrir();

        if (!ExisteColumna(cn, "Anulada"))
        {
            using var alter = cn.Comando("ALTER TABLE Venta ADD COLUMN Anulada INTEGER NOT NULL DEFAULT 0;");
            alter.ExecuteNonQuery();
        }

        if (!ExisteColumna(cn, "FechaAnulacion"))
        {
            using var alter = cn.Comando("ALTER TABLE Venta ADD COLUMN FechaAnulacion TEXT;");
            alter.ExecuteNonQuery();
        }
    }

    private static bool ExisteColumna(System.Data.Common.DbConnection cn, string columna)
    {
        using var cmd = cn.Comando("SELECT COUNT(*) FROM pragma_table_info('Venta') WHERE name = @col;")
                          .Param("@col", columna);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }
}
