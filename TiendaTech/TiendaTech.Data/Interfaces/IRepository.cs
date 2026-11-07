using TiendaTech.Models;

namespace TiendaTech.Data;

// Contrato generico: las capas superiores dependen de esta abstraccion, no de SQLite (DIP)
public interface IRepository<T> where T : IEntidad
{
    List<T> ObtenerTodos();
    void Agregar(T entidad);
    void Eliminar(int id);
}
