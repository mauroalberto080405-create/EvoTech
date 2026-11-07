using System.Data.Common;
using System.Globalization;

namespace TiendaTech.Data;

// Atajos para escribir menos codigo ADO.NET en los repositorios
internal static class DbExtensiones
{
    public const string FormatoFecha = "yyyy-MM-dd HH:mm:ss";

    public static DbCommand Comando(this DbConnection cn, string sql, DbTransaction? tx = null)
    {
        var cmd = cn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Transaction = tx;
        return cmd;
    }

    public static DbCommand Param(this DbCommand cmd, string nombre, object? valor)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = nombre;
        p.Value = valor ?? DBNull.Value;
        cmd.Parameters.Add(p);
        return cmd;
    }

    public static int UltimoId(this DbConnection cn, DbTransaction? tx = null)
    {
        using var cmd = cn.Comando("SELECT last_insert_rowid();", tx);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    // Ejecuta una consulta COUNT con un parametro @id
    public static int Contar(this DbConnection cn, string sql, int id)
    {
        using var cmd = cn.Comando(sql).Param("@id", id);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    // Ejecuta un DELETE ... WHERE Id = @id
    public static void EliminarPorId(this DbConnection cn, string tabla, int id)
    {
        using var cmd = cn.Comando($"DELETE FROM {tabla} WHERE Id = @id;").Param("@id", id);
        cmd.ExecuteNonQuery();
    }

    public static string Texto(this DbDataReader r, string col) =>
        r[col] is DBNull ? "" : Convert.ToString(r[col], CultureInfo.InvariantCulture) ?? "";

    public static int Entero(this DbDataReader r, string col) =>
        r[col] is DBNull ? 0 : Convert.ToInt32(r[col], CultureInfo.InvariantCulture);

    public static int? EnteroNulo(this DbDataReader r, string col) =>
        r[col] is DBNull ? null : Convert.ToInt32(r[col], CultureInfo.InvariantCulture);

    public static decimal Decimal(this DbDataReader r, string col) =>
        r[col] is DBNull ? 0m : Convert.ToDecimal(r[col], CultureInfo.InvariantCulture);

    public static DateTime Fecha(this DbDataReader r, string col) =>
        DateTime.ParseExact(r.Texto(col), FormatoFecha, CultureInfo.InvariantCulture);
}
