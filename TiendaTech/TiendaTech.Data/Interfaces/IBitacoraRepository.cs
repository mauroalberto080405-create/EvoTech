using TiendaTech.Models;

namespace TiendaTech.Data;

// Registro y consulta de los intentos de inicio de sesión
public interface IBitacoraRepository
{
    void Agregar(RegistroBitacora registro);
    List<RegistroBitacora> ObtenerEnRango(DateTime desde, DateTime hasta);
}
