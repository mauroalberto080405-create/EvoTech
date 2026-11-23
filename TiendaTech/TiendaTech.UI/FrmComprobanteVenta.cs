using System.Drawing.Printing;
using TiendaTech.Models;

namespace TiendaTech.UI;

// PANTALLA NUEVA (T4) - Comprobante de venta (boleta) con vista previa de impresión.
// Solo muestra datos que ya trae el Historial de ventas: no necesita repositorio ni servicio nuevo.
public class FrmComprobanteVenta : FormBase
{
    private readonly VentaResumen _venta;
    private readonly List<DetalleVenta> _detalles;
    private int _filaImpresa; // por dónde va la impresión si el detalle ocupa más de una página

    public FrmComprobanteVenta(VentaResumen venta, List<DetalleVenta> detalles)
    {
        _venta = venta;
        _detalles = detalles;
        Text = $"Comprobante de venta N° {venta.Id:D6}";
        ClientSize = new Size(620, 600);
        StartPosition = FormStartPosition.CenterScreen;

        UiHelper.Etiqueta(this, "EvoTech", 20, 12, true);
        var lblTipo = UiHelper.Etiqueta(this, $"Comprobante de venta N° {venta.Id:D6}", 20, 50);
        lblTipo.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        lblTipo.ForeColor = Tema.Gris;

        // Venta anulada: aviso grande en rojo (solo se ve si corresponde)
        var lblAnulada = UiHelper.Etiqueta(this, "VENTA ANULADA", 400, 20);
        lblAnulada.Font = new Font("Segoe UI", 16, FontStyle.Bold);
        lblAnulada.ForeColor = Tema.Peligro;
        lblAnulada.Visible = venta.Anulada;

        UiHelper.Etiqueta(this, $"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}", 20, 85);
        UiHelper.Etiqueta(this, $"Cliente: {venta.Cliente}", 20, 112);
        UiHelper.Etiqueta(this, $"Vendedor: {venta.Vendedor}", 20, 139);

        var grid = UiHelper.Grilla(this, 20, 175, 580, 240);
        UiHelper.Columna(grid, "NombreProducto", "Producto", peso: 200);
        UiHelper.Columna(grid, "Cantidad", "Cant.", peso: 45);
        UiHelper.Columna(grid, "PrecioUnitario", "Precio S/", "N2", 70);
        UiHelper.Columna(grid, "Subtotal", "Subtotal S/", "N2", 70);
        grid.DataSource = detalles;
        grid.ClearSelection();

        // Totales alineados a la derecha
        Totales("Subtotal:", $"S/ {venta.Subtotal:N2}", 430, false);
        Totales("Descuento:", $"- S/ {venta.Descuento:N2}", 462, false);
        Totales("TOTAL:", $"S/ {venta.Total:N2}", 494, true);

        var btnImprimir = UiHelper.Boton(this, "Imprimir", 340, 548, 130);
        btnImprimir.Click += (_, _) => Imprimir();
        var btnCerrar = UiHelper.Boton(this, "Cerrar", 480, 548, 120, color: Tema.Secundario);
        btnCerrar.Click += (_, _) => Close();
    }

    private void Totales(string etiqueta, string valor, int y, bool destacado)
    {
        var fuente = new Font("Segoe UI", destacado ? 13 : 10.5F, destacado ? FontStyle.Bold : FontStyle.Regular);
        var color = destacado ? Tema.Primario : Tema.Oscuro;
        Controls.Add(new Label { Text = etiqueta, Left = 330, Top = y, Width = 120, Height = 28, Font = fuente, ForeColor = color, TextAlign = ContentAlignment.MiddleRight });
        Controls.Add(new Label { Text = valor, Left = 455, Top = y, Width = 145, Height = 28, Font = fuente, ForeColor = color, TextAlign = ContentAlignment.MiddleRight });
    }

    // ---------- Impresión (PrintDocument + PrintPreviewDialog vienen en Windows Forms) ----------

    private void Imprimir()
    {
        try
        {
            _filaImpresa = 0;
            using var doc = new PrintDocument { DocumentName = $"Comprobante {_venta.Id:D6}" };
            doc.PrintPage += DibujarPagina;
            using var vista = new PrintPreviewDialog { Document = doc, ClientSize = new Size(900, 700) };
            vista.ShowDialog(this);
        }
        catch (InvalidPrinterException)
        {
            UiHelper.Error("No hay una impresora disponible. Instala o activa una impresora (o \"Microsoft Print to PDF\") e inténtalo de nuevo.");
        }
    }

    private void DibujarPagina(object? sender, PrintPageEventArgs e)
    {
        var g = e.Graphics!;
        var m = e.MarginBounds;
        float x = m.Left, y = m.Top, ancho = m.Width;

        using var titulo = new Font("Segoe UI", 20, FontStyle.Bold);
        using var subtitulo = new Font("Segoe UI", 12, FontStyle.Bold);
        using var normal = new Font("Segoe UI", 10);
        using var negrita = new Font("Segoe UI", 10, FontStyle.Bold);
        using var grande = new Font("Segoe UI", 14, FontStyle.Bold);
        using var anulada = new Font("Segoe UI", 18, FontStyle.Bold);
        using var pincelNegro = new SolidBrush(Tema.Oscuro);
        using var pincelNaranja = new SolidBrush(Tema.Primario);
        using var pincelRojo = new SolidBrush(Tema.Peligro);
        using var linea = new Pen(Tema.Gris, 1);
        var derecha = new StringFormat { Alignment = StringAlignment.Far };

        // Encabezado
        g.DrawString("EvoTech", titulo, pincelNaranja, x, y);
        y += titulo.GetHeight(g) + 4;
        g.DrawString($"Comprobante de venta N° {_venta.Id:D6}", subtitulo, pincelNegro, x, y);
        if (_venta.Anulada)
            g.DrawString("VENTA ANULADA", anulada, pincelRojo, new RectangleF(x, y - 10, ancho, 40), derecha);
        y += subtitulo.GetHeight(g) + 10;

        g.DrawString($"Fecha: {_venta.Fecha:dd/MM/yyyy HH:mm}", normal, pincelNegro, x, y);
        y += normal.GetHeight(g) + 2;
        g.DrawString($"Cliente: {_venta.Cliente}", normal, pincelNegro, x, y);
        y += normal.GetHeight(g) + 2;
        g.DrawString($"Vendedor: {_venta.Vendedor}", normal, pincelNegro, x, y);
        y += normal.GetHeight(g) + 12;

        // Encabezado de la tabla
        float xCant = x + ancho * 0.52f, xPrecio = x + ancho * 0.74f;
        float wCant = ancho * 0.12f, wPrecio = ancho * 0.20f, wSub = ancho * 0.26f;
        g.DrawLine(linea, x, y, x + ancho, y);
        y += 3;
        g.DrawString("Producto", negrita, pincelNegro, x, y);
        g.DrawString("Cant.", negrita, pincelNegro, new RectangleF(xCant, y, wCant, 24), derecha);
        g.DrawString("Precio S/", negrita, pincelNegro, new RectangleF(xPrecio - 5, y, wPrecio, 24), derecha);
        g.DrawString("Subtotal S/", negrita, pincelNegro, new RectangleF(x + ancho - wSub, y, wSub, 24), derecha);
        y += negrita.GetHeight(g) + 3;
        g.DrawLine(linea, x, y, x + ancho, y);
        y += 4;

        // Filas: se deja espacio abajo para los totales; si no entra todo, sigue en otra página
        float alturaFila = normal.GetHeight(g) + 4;
        float reservaTotales = 110;
        while (_filaImpresa < _detalles.Count)
        {
            if (y + alturaFila > m.Bottom - reservaTotales)
            {
                e.HasMorePages = true;
                return;
            }
            var d = _detalles[_filaImpresa];
            var nombre = new RectangleF(x, y, xCant - x - 5, alturaFila);
            g.DrawString(d.NombreProducto, normal, pincelNegro, nombre, new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap });
            g.DrawString(d.Cantidad.ToString(), normal, pincelNegro, new RectangleF(xCant, y, wCant, alturaFila), derecha);
            g.DrawString(d.PrecioUnitario.ToString("N2"), normal, pincelNegro, new RectangleF(xPrecio - 5, y, wPrecio, alturaFila), derecha);
            g.DrawString(d.Subtotal.ToString("N2"), normal, pincelNegro, new RectangleF(x + ancho - wSub, y, wSub, alturaFila), derecha);
            y += alturaFila;
            _filaImpresa++;
        }

        // Totales (solo en la última página)
        y += 4;
        g.DrawLine(linea, x, y, x + ancho, y);
        y += 6;
        FilaTotal(g, "Subtotal:", $"S/ {_venta.Subtotal:N2}", normal, pincelNegro, x, ancho, ref y);
        FilaTotal(g, "Descuento:", $"- S/ {_venta.Descuento:N2}", normal, pincelNegro, x, ancho, ref y);
        FilaTotal(g, "TOTAL:", $"S/ {_venta.Total:N2}", grande, pincelNaranja, x, ancho, ref y);

        e.HasMorePages = false;
        _filaImpresa = 0; // por si se vuelve a generar la vista previa
    }

    private static void FilaTotal(Graphics g, string etiqueta, string valor, Font fuente, Brush pincel,
                                  float x, float ancho, ref float y)
    {
        var derecha = new StringFormat { Alignment = StringAlignment.Far };
        g.DrawString(etiqueta, fuente, pincel, new RectangleF(x, y, ancho * 0.74f, 30), derecha);
        g.DrawString(valor, fuente, pincel, new RectangleF(x + ancho * 0.74f, y, ancho * 0.26f, 30), derecha);
        y += fuente.GetHeight(g) + 4;
    }
}
