namespace TiendaTech.Models;

public class Usuario : IEntidad
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = "";
    public string Clave { get; set; } = "";
    public string Rol { get; set; } = "Vendedor";
}
