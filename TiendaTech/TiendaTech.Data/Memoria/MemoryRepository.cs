using TiendaTech.Models;

namespace TiendaTech.Data;

// Version SIN base de datos (en memoria). Se conserva para demostrar DIP:
// en Program.cs se puede cambiar la version SQL por esta sin tocar servicios ni pantallas.
public class MemoryRepository<T> : IRepository<T> where T : IEntidad
{
    protected readonly List<T> Items = new();

    public List<T> ObtenerTodos() => Items.ToList();

    public void Agregar(T entidad)
    {
        entidad.Id = Items.Count == 0 ? 1 : Items.Max(x => x.Id) + 1;
        Items.Add(entidad);
    }

    public void Eliminar(int id) => Items.RemoveAll(x => x.Id == id);
}
