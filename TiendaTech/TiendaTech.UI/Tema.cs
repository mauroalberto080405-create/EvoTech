namespace TiendaTech.UI;

// Colores de la aplicacion en un solo lugar (cambiar aqui cambia todas las pantallas)
public static class Tema
{
    public static readonly Color Primario = Color.FromArgb(242, 92, 5);     // naranja
    public static readonly Color Secundario = Color.FromArgb(26, 158, 143); // turquesa
    public static readonly Color Oscuro = Color.FromArgb(31, 36, 48);
    public static readonly Color Gris = Color.FromArgb(91, 100, 117);
    public static readonly Color Fondo = Color.FromArgb(247, 248, 250);
    public static readonly Color FilaAlterna = Color.FromArgb(255, 244, 236);
    public static readonly Color Peligro = Color.FromArgb(217, 48, 37);     // rojo (eliminar)
    public static readonly Color Excel = Color.FromArgb(33, 115, 70);       // verde Excel
}

// Todas las pantallas heredan de aquí: fondo claro, franja naranja,
// se agrandan según la escala de Windows y se estiran al maximizar.
public class FormBase : Form
{
    // 1.0 = tamaño normal, 1.1 = 10% más grande
    private const float Escala = 1.1f;

    // Posición, tamaño y letra original de cada control
    private readonly Dictionary<Control, (Rectangle Limites, float Letra)> _original = new();
    private Size _tamanoOriginal;

    public FormBase()
    {
        AutoScaleMode = AutoScaleMode.None;
        Font = new Font("Segoe UI", 10F);
        BackColor = Tema.Fondo;
        Controls.Add(new Panel { Dock = DockStyle.Top, Height = 5, BackColor = Tema.Primario });
    }

    protected override void OnLoad(EventArgs e)
    {
        float factor = DeviceDpi / 96f * Escala;
        Scale(new SizeF(factor, factor));
        base.OnLoad(e);
        CenterToScreen();

        // Guarda cómo quedó todo para poder estirarlo después
        _tamanoOriginal = ClientSize;
        foreach (Control c in Controls)
            if (c.Dock == DockStyle.None)
                _original[c] = (c.Bounds, c.Font.Size);

        Resize += (_, _) => Reacomodar();
    }

    // Cada vez que cambia el tamaño, estira posiciones, tamaños y letras
    private void Reacomodar()
    {
        if (_tamanoOriginal.Width == 0 || WindowState == FormWindowState.Minimized) return;

        float fx = (float)ClientSize.Width / _tamanoOriginal.Width;
        float fy = (float)ClientSize.Height / _tamanoOriginal.Height;
        float fl = Math.Max(1f, Math.Min(fx, fy)); // la letra crece, nunca se achica

        SuspendLayout();
        foreach (var (c, o) in _original)
        {
            c.Bounds = new Rectangle((int)(o.Limites.X * fx), (int)(o.Limites.Y * fy),
                                     (int)(o.Limites.Width * fx), (int)(o.Limites.Height * fy));
            c.Font = new Font(c.Font.FontFamily, o.Letra * fl, c.Font.Style);

            // Las tablas también agrandan encabezado y filas
            if (c is DataGridView g)
            {
                g.ColumnHeadersDefaultCellStyle.Font = new Font(c.Font, FontStyle.Bold);
                g.ColumnHeadersHeight = (int)(c.Font.Height * 1.8);
                g.RowTemplate.Height = (int)(c.Font.Height * 1.5);
                foreach (DataGridViewRow fila in g.Rows) fila.Height = g.RowTemplate.Height;
            }
        }
        ResumeLayout();
    }
}
