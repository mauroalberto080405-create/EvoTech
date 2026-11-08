namespace TiendaTech.Models;

public class Usuario : IEntidad
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = "";
    public string Clave { get; set; } = ""; // en la BD se guarda el hash, no la clave en texto plano
    public string Rol { get; set; } = "Vendedor";
}
