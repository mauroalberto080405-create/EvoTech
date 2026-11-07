namespace TiendaTech.Models;

public class Cliente : IEntidad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Documento { get; set; } = "";
    public string Telefono { get; set; } = "";
    public override string ToString() => Nombre;
}
