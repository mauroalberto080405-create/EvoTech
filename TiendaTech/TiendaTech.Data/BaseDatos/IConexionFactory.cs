using System.Data.Common;

namespace TiendaTech.Data;

// DIP: los repositorios piden "una conexion abierta" sin saber si es SQLite, SQL Server u otro motor
public interface IConexionFactory
{
    string Descripcion { get; }
    DbConnection Abrir();
}
