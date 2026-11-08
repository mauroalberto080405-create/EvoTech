namespace TiendaTech.Data;

// Asegura que la tabla Compra tenga la columna NumeroDocumento (N° de factura/guía del proveedor).
// - BD nueva o antigua: si falta la columna, la agrega SIN borrar datos (las compras antiguas quedan sin N°).
// - Crea un índice único para que un proveedor no tenga dos ingresos con el mismo documento.
// Está separado de DbInicializador para que esta mejora no toque archivos de otros módulos.
public static class EsquemaCompras
{
    public static void Asegurar(IConexionFactory fabrica)
    {
        using var cn = fabrica.Abrir();

        using (var cmd = cn.Comando("SELECT COUNT(*) FROM pragma_table_info('Compra') WHERE name = 'NumeroDocumento';"))
        {
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
            {
                using var alter = cn.Comando("ALTER TABLE Compra ADD COLUMN NumeroDocumento TEXT;");
                alter.ExecuteNonQuery();
            }
        }

        using var indice = cn.Comando("""
            CREATE UNIQUE INDEX IF NOT EXISTS UX_Compra_Proveedor_Documento
            ON Compra (ProveedorId, NumeroDocumento)
            WHERE NumeroDocumento IS NOT NULL;
            """);
        indice.ExecuteNonQuery();
    }
}
