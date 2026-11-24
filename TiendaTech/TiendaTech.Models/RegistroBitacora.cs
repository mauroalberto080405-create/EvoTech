namespace TiendaTech.Models;

// Un intento de inicio de sesión (nunca incluye la clave escrita)
public class RegistroBitacora : IEntidad
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Usuario { get; set; } = "";  // usuario tal como se escribió en el login
    public string Evento { get; set; } = "";   // "Inicio exitoso" / "Clave incorrecta" / "Usuario no existe"
}
