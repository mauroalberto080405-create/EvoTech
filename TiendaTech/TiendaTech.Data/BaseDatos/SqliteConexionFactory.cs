using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace TiendaTech.Data;

// Implementacion concreta para SQLite (archivo TiendaTech.db junto al ejecutable)
public class SqliteConexionFactory : IConexionFactory
{
    private readonly string _rutaArchivo;

    public SqliteConexionFactory(string rutaArchivo) => _rutaArchivo = rutaArchivo;

    public string Descripcion => $"SQLite - {Path.GetFileName(_rutaArchivo)}";

    public DbConnection Abrir()
    {
        var cn = new SqliteConnection($"Data Source={_rutaArchivo}");
        cn.Open();
        using var cmd = cn.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = ON;"; // activa las llaves foraneas
        cmd.ExecuteNonQuery();
        return cn;
    }
}
