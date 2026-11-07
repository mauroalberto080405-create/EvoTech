namespace TiendaTech.Business;

// OCP: para una nueva regla de descuento se crea una clase nueva; VentaService NO se modifica.
public interface IDescuento
{
    decimal Calcular(decimal subtotal);
}

public class SinDescuento : IDescuento
{
    public decimal Calcular(decimal subtotal) => 0m;
}

// Regla actual de la tienda: 5% si la compra llega a S/ 1000
public class DescuentoPorMonto : IDescuento
{
    public decimal Calcular(decimal subtotal) => subtotal >= 1000m ? Math.Round(subtotal * 0.05m, 2) : 0m;
}

// Extension agregada despues (campanas como Cyber Days): porcentaje fijo a toda compra
public class DescuentoCampania : IDescuento
{
    private readonly decimal _porcentaje;
    public DescuentoCampania(decimal porcentaje) => _porcentaje = porcentaje;
    public decimal Calcular(decimal subtotal) => Math.Round(subtotal * _porcentaje, 2);
}
