using TiendaTech.Models;

namespace TiendaTech.Data;

public interface IClienteRepository : IRepository<Cliente>
{
    bool ExisteDocumento(string documento);
    bool TieneVentas(int id);
}
