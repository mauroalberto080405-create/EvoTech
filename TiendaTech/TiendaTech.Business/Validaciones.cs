namespace TiendaTech.Business;

// Reglas de formato reutilizadas por varios servicios (una sola fuente de verdad)
public static class Validaciones
{
    public const int MaxTelefono = 9;

    // Usuario y clave: sin letras mayúsculas ni espacios (al inicio, en medio o al final)
    public static bool CaracterPermitidoCredencial(char c) => !char.IsUpper(c) && !char.IsWhiteSpace(c);
    public static bool CredencialValida(string texto) => texto.All(CaracterPermitidoCredencial);
    public const string MensajeCredencial =
        "El usuario y la clave no pueden tener letras mayúsculas ni espacios. Escríbalos en minúsculas y sin espacios.";

    // Teléfono: solo dígitos 0-9, máximo 9
    public static bool CaracterPermitidoTelefono(char c) => c >= '0' && c <= '9';
    public static bool TelefonoValido(string telefono) =>
        telefono.Length <= MaxTelefono && telefono.All(CaracterPermitidoTelefono);
    public const string MensajeTelefono = "El teléfono solo puede tener números (0-9), sin espacios ni guiones, y máximo 9 dígitos.";
}
