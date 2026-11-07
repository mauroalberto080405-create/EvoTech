namespace TiendaTech.Models;

public class Proveedor : IEntidad
{
    public int Id { get; set; }
    public string Empresa { get; set; } = "";
    public string Ruc { get; set; } = "";
    public string Telefono { get; set; } = "";
    public override string ToString() => Empresa;
}
