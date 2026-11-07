namespace TiendaTech.UI;

// Metodos auxiliares para crear controles por codigo (evita archivos .Designer y conflictos en Git)
public static class UiHelper
{
    public static Label Etiqueta(Control padre, string texto, int x, int y, bool titulo = false)
    {
        var l = new Label { Text = texto, Left = x, Top = y, AutoSize = true };
        if (titulo) { l.Font = new Font("Segoe UI", 16, FontStyle.Bold); l.ForeColor = Tema.Oscuro; }
        padre.Controls.Add(l);
        return l;
    }

    public static TextBox Caja(Control padre, int x, int y, int ancho = 200, bool clave = false)
    {
        var t = new TextBox { Left = x, Top = y, Width = ancho, UseSystemPasswordChar = clave };
        padre.Controls.Add(t);
        return t;
    }

    // Boton plano de color (naranja por defecto); se pone gris si se deshabilita
    public static Button Boton(Control padre, string texto, int x, int y, int ancho = 120, int alto = 32, Color? color = null)
    {
        var fondo = color ?? Tema.Primario;
        var b = new Button
        {
            Text = texto, Left = x, Top = y, Width = ancho, Height = alto,
            FlatStyle = FlatStyle.Flat, BackColor = fondo, ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.75F, FontStyle.Bold), Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderSize = 0;
        b.EnabledChanged += (_, _) => b.BackColor = b.Enabled ? fondo : Color.FromArgb(205, 208, 214);
        padre.Controls.Add(b);
        return b;
    }

    public static ComboBox Combo(Control padre, int x, int y, int ancho)
    {
        var c = new ComboBox { Left = x, Top = y, Width = ancho, DropDownStyle = ComboBoxStyle.DropDownList };
        padre.Controls.Add(c);
        return c;
    }

    public static DataGridView Grilla(Control padre, int x, int y, int ancho, int alto)
    {
        var g = new DataGridView
        {
            Left = x, Top = y, Width = ancho, Height = alto,
            ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false,
            AutoGenerateColumns = false, MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 32,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Color.FromArgb(225, 228, 232)
        };
        // Encabezado oscuro, filas alternas naranja claro, seleccion turquesa
        g.ColumnHeadersDefaultCellStyle.BackColor = Tema.Oscuro;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Tema.Oscuro;
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        g.AlternatingRowsDefaultCellStyle.BackColor = Tema.FilaAlterna;
        g.DefaultCellStyle.SelectionBackColor = Tema.Secundario;
        g.DefaultCellStyle.SelectionForeColor = Color.White;
        g.RowTemplate.Height = 26;
        padre.Controls.Add(g);
        return g;
    }

    // Agrega una columna enlazada a una propiedad del objeto (formato "N2" = 2 decimales)
    public static void Columna(DataGridView g, string propiedad, string titulo, string? formato = null, int peso = 100)
    {
        var col = new DataGridViewTextBoxColumn { DataPropertyName = propiedad, HeaderText = titulo, FillWeight = peso };
        if (formato != null) col.DefaultCellStyle.Format = formato;
        g.Columns.Add(col);
    }

    // Etiqueta roja pequeña para avisos debajo de un campo (oculta hasta que haga falta)
    public static Label Aviso(Control padre, int x, int y)
    {
        var l = Etiqueta(padre, "", x, y);
        l.ForeColor = Tema.Peligro;
        l.Font = new Font("Segoe UI", 8.25F);
        l.Visible = false;
        return l;
    }

    // Deja escribir o pegar solo los caracteres permitidos y muestra el aviso si se intenta otro
    public static void Filtrar(TextBox caja, Func<char, bool> permitido, Label aviso, string mensaje)
    {
        caja.KeyPress += (_, e) =>
        {
            if (char.IsControl(e.KeyChar)) return;          // Borrar, Enter, Ctrl+V, etc.
            if (permitido(e.KeyChar)) { aviso.Visible = false; return; }
            e.Handled = true;                                // no se escribe
            aviso.Text = mensaje;
            aviso.Visible = true;
        };
        caja.TextChanged += (_, _) =>                       // cubre lo que se pega con Ctrl+V
        {
            var limpio = new string(caja.Text.Where(permitido).ToArray());
            if (limpio == caja.Text) return;
            int quitados = caja.Text.Length - limpio.Length;
            int pos = Math.Max(0, caja.SelectionStart - quitados);
            caja.Text = limpio;
            caja.SelectionStart = Math.Min(pos, limpio.Length);
            aviso.Text = mensaje;
            aviso.Visible = true;
        };
    }

    // Pregunta Sí/No antes de una accion que no se puede deshacer
    public static bool Confirmar(string mensaje) =>
        MessageBox.Show(mensaje, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2) == DialogResult.Yes;

    public static void Error(string mensaje) =>
        MessageBox.Show(mensaje, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public static void ErrorBd(Exception ex) =>
        MessageBox.Show("Error al acceder a la base de datos:\n" + ex.Message, "Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
}
