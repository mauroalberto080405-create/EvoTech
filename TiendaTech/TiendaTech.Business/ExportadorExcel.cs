using System.Globalization;
using System.IO.Compression;
using System.Security;
using System.Text;

namespace TiendaTech.Business;

// Una hoja del libro: encabezados + filas (string, int, decimal o DateTime)
public class HojaExcel
{
    public string Nombre { get; }
    public string[] Encabezados { get; }
    public double[] Anchos { get; }
    public List<object[]> Filas { get; } = new();

    public HojaExcel(string nombre, string[] encabezados, double[] anchos)
    {
        Nombre = nombre.Length > 31 ? nombre[..31] : nombre; // límite de Excel
        Encabezados = encabezados;
        Anchos = anchos;
    }
}

// Genera un archivo .xlsx (Office Open XML) válido usando solo .NET (System.IO.Compression).
// No necesita Excel instalado ni librerías externas.
public static class ExportadorExcel
{
    // Estilos: 0 normal, 1 encabezado, 2 moneda #,##0.00, 3 fecha y hora, 4 entero, 5 solo fecha
    public static void Guardar(string ruta, IList<HojaExcel> hojas)
    {
        if (hojas.Count == 0) throw new ArgumentException("No hay datos para exportar.");

        // Se escribe primero en un archivo temporal: si algo falla, no queda un .xlsx a medias
        var temporal = ruta + ".tmp";
        try
        {
            using (var fs = new FileStream(temporal, FileMode.Create, FileAccess.Write))
            using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                Escribir(zip, "[Content_Types].xml", TiposContenido(hojas.Count));
                Escribir(zip, "_rels/.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                    "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                    "</Relationships>");
                Escribir(zip, "xl/workbook.xml", Libro(hojas));
                Escribir(zip, "xl/_rels/workbook.xml.rels", RelacionesLibro(hojas.Count));
                Escribir(zip, "xl/styles.xml", Estilos());
                for (int i = 0; i < hojas.Count; i++)
                    Escribir(zip, $"xl/worksheets/sheet{i + 1}.xml", Hoja(hojas[i]));
            }
            File.Move(temporal, ruta, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporal)) File.Delete(temporal);
        }
    }

    private static void Escribir(ZipArchive zip, string nombre, string contenido)
    {
        var entrada = zip.CreateEntry(nombre, CompressionLevel.Optimal);
        using var w = new StreamWriter(entrada.Open(), new UTF8Encoding(false));
        w.Write(contenido);
    }

    private static string TiposContenido(int n)
    {
        var sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
            "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
            "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
            "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
            "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>");
        for (int i = 1; i <= n; i++)
            sb.Append($"<Override PartName=\"/xl/worksheets/sheet{i}.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>");
        return sb.Append("</Types>").ToString();
    }

    private static string Libro(IList<HojaExcel> hojas)
    {
        var sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" " +
            "xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets>");
        for (int i = 0; i < hojas.Count; i++)
            sb.Append($"<sheet name=\"{Xml(hojas[i].Nombre)}\" sheetId=\"{i + 1}\" r:id=\"rId{i + 1}\"/>");
        return sb.Append("</sheets></workbook>").ToString();
    }

    private static string RelacionesLibro(int n)
    {
        var sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
        for (int i = 1; i <= n; i++)
            sb.Append($"<Relationship Id=\"rId{i}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet{i}.xml\"/>");
        sb.Append($"<Relationship Id=\"rId{n + 1}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>");
        return sb.Append("</Relationships>").ToString();
    }

    private static string Estilos() =>
        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
        "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">" +
        "<numFmts count=\"2\"><numFmt numFmtId=\"164\" formatCode=\"dd/mm/yyyy hh:mm\"/><numFmt numFmtId=\"165\" formatCode=\"dd/mm/yyyy\"/></numFmts>" +
        "<fonts count=\"2\"><font><sz val=\"11\"/><name val=\"Calibri\"/></font>" +
        "<font><b/><sz val=\"11\"/><color rgb=\"FFFFFFFF\"/><name val=\"Calibri\"/></font></fonts>" +
        "<fills count=\"3\"><fill><patternFill patternType=\"none\"/></fill><fill><patternFill patternType=\"gray125\"/></fill>" +
        "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFF25C05\"/><bgColor indexed=\"64\"/></patternFill></fill></fills>" +
        "<borders count=\"1\"><border><left/><right/><top/><bottom/><diagonal/></border></borders>" +
        "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>" +
        "<cellXfs count=\"6\">" +
        "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>" +
        "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"2\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\"/>" +
        "<xf numFmtId=\"4\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\"/>" +
        "<xf numFmtId=\"164\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\"/>" +
        "<xf numFmtId=\"1\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\"/>" +
        "<xf numFmtId=\"165\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\"/>" +
        "</cellXfs><cellStyles count=\"1\"><cellStyle name=\"Normal\" xfId=\"0\" builtinId=\"0\"/></cellStyles>" +
        "</styleSheet>";

    private static string Hoja(HojaExcel hoja)
    {
        var sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">" +
            // Encabezado fijo al hacer scroll
            "<sheetViews><sheetView workbookViewId=\"0\"><pane ySplit=\"1\" topLeftCell=\"A2\" activePane=\"bottomLeft\" state=\"frozen\"/></sheetView></sheetViews>");
        sb.Append("<cols>");
        for (int c = 0; c < hoja.Anchos.Length; c++)
            sb.Append($"<col min=\"{c + 1}\" max=\"{c + 1}\" width=\"{hoja.Anchos[c].ToString(CultureInfo.InvariantCulture)}\" customWidth=\"1\"/>");
        sb.Append("</cols><sheetData>");

        sb.Append("<row r=\"1\">");
        for (int c = 0; c < hoja.Encabezados.Length; c++)
            sb.Append(CeldaTexto(Ref(c, 1), hoja.Encabezados[c], estilo: 1));
        sb.Append("</row>");

        for (int f = 0; f < hoja.Filas.Count; f++)
        {
            int fila = f + 2;
            sb.Append($"<row r=\"{fila}\">");
            var valores = hoja.Filas[f];
            for (int c = 0; c < valores.Length; c++)
                sb.Append(Celda(Ref(c, fila), valores[c]));
            sb.Append("</row>");
        }
        return sb.Append("</sheetData></worksheet>").ToString();
    }

    private static string Celda(string r, object? valor) => valor switch
    {
        null => "",
        string s => CeldaTexto(r, s, 0),
        DateTime d => CeldaNumero(r, d.ToOADate(), d.TimeOfDay == TimeSpan.Zero ? 5 : 3),
        decimal m => CeldaNumero(r, (double)m, 2),
        double x => CeldaNumero(r, x, 2),
        int i => CeldaNumero(r, i, 4),
        long l => CeldaNumero(r, l, 4),
        _ => CeldaTexto(r, Convert.ToString(valor, CultureInfo.InvariantCulture) ?? "", 0)
    };

    private static string CeldaTexto(string r, string texto, int estilo) =>
        $"<c r=\"{r}\" t=\"inlineStr\"{(estilo > 0 ? $" s=\"{estilo}\"" : "")}><is><t xml:space=\"preserve\">{Xml(texto)}</t></is></c>";

    private static string CeldaNumero(string r, double numero, int estilo) =>
        $"<c r=\"{r}\" s=\"{estilo}\"><v>{numero.ToString("R", CultureInfo.InvariantCulture)}</v></c>";

    // Columna 0 -> A, 25 -> Z, 26 -> AA
    private static string Ref(int columna, int fila)
    {
        var letras = "";
        for (int c = columna + 1; c > 0; c = (c - 1) / 26)
            letras = (char)('A' + (c - 1) % 26) + letras;
        return letras + fila;
    }

    private static string Xml(string texto) => SecurityElement.Escape(texto) ?? "";
}
