using TiendaTech.Business;
using TiendaTech.Models;

namespace TiendaTech.UI;

// Bitácora de accesos: intentos de inicio de sesión (solo Administrador; el botón lo agrega el líder en el menú)
public class FrmBitacora : FormBase
{
    private readonly IBitacoraService _service;
    private readonly DateTimePicker _dtpDesde, _dtpHasta;
    private readonly DataGridView _grid;
    private readonly Label _lblResumen;

    public FrmBitacora(IBitacoraService service, ISesion sesion)
    {
        _service = service;
        Text = "Bitácora de accesos";
        ClientSize = new Size(700, 480);
        StartPosition = FormStartPosition.CenterScreen;

        // Segunda barrera: aunque se abra desde otro lado, solo el administrador ve la bitácora
        if (sesion.UsuarioActual?.Rol != "Administrador")
        {
            Load += (_, _) => { UiHelper.Error("Solo el administrador puede ver la bitácora de accesos."); Close(); };
        }

        var hoy = DateTime.Today;
        UiHelper.Etiqueta(this, "Desde:", 20, 22);
        _dtpDesde = new DateTimePicker { Left = 70, Top = 18, Width = 130, Format = DateTimePickerFormat.Short, Value = hoy.AddDays(-7) };
        Controls.Add(_dtpDesde);
        UiHelper.Etiqueta(this, "Hasta:", 220, 22);
        _dtpHasta = new DateTimePicker { Left = 270, Top = 18, Width = 130, Format = DateTimePickerFormat.Short, Value = hoy };
        Controls.Add(_dtpHasta);
        var btn = UiHelper.Boton(this, "Actualizar", 420, 14, 120, color: Tema.Secundario);
        btn.Click += (_, _) => Cargar();

        _grid = UiHelper.Grilla(this, 20, 60, 660, 360);
        UiHelper.Columna(_grid, "Fecha", "Fecha y hora", "dd/MM/yyyy HH:mm:ss", 110);
        UiHelper.Columna(_grid, "Usuario", "Usuario escrito", peso: 120);
        UiHelper.Columna(_grid, "Evento", "Evento", peso: 120);
        _grid.CellFormatting += PintarEvento;

        _lblResumen = UiHelper.Etiqueta(this, "", 20, 435);
        _lblResumen.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        _lblResumen.ForeColor = Tema.Secundario;

        Load += (_, _) => { if (!IsDisposed) Cargar(); };
    }

    private void Cargar()
    {
        try
        {
            var lista = _service.Listar(_dtpDesde.Value, _dtpHasta.Value);
            _grid.DataSource = lista;
            int fallidos = lista.Count(r => r.Evento != BitacoraService.InicioExitoso);
            _lblResumen.Text = lista.Count == 0
                ? "No hay intentos de inicio de sesión en ese rango."
                : $"{lista.Count} intento(s): {lista.Count - fallidos} exitoso(s), {fallidos} fallido(s).";
        }
        catch (ArgumentException ex) { UiHelper.Error(ex.Message); }
        catch (Exception ex) { UiHelper.ErrorBd(ex); }
    }

    // Intentos fallidos en rojo
    private void PintarEvento(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex >= 0 && _grid.Rows[e.RowIndex].DataBoundItem is RegistroBitacora r && r.Evento != BitacoraService.InicioExitoso)
            e.CellStyle.ForeColor = Color.Firebrick;
    }
}
