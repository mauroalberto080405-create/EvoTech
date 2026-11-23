
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TiendaTech.Business;
using TiendaTech.Models;

namespace TiendaTech.UI;

// Ficha del cliente - Castillo
public class FrmFichaCliente : FormBase
{
    private readonly Cliente _cliente;
    private readonly IVentaService _ventaService;

    public FrmFichaCliente(
        Cliente cliente,
        IVentaService ventaService)
    {
        _cliente = cliente;
        _ventaService = ventaService;

        Text = "Ficha del cliente - " + cliente.Nombre;
        ClientSize = new Size(720, 530);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        // DATOS PERSONALES
        var titulo = new Label
        {
            Text = "FICHA DEL CLIENTE",
            Location = new Point(20, 15),
            Size = new Size(650, 30),
            Font = new Font("Segoe UI", 16, FontStyle.Bold)
        };
        Controls.Add(titulo);

        AgregarDato("Nombre:", cliente.Nombre, 55);
        AgregarDato("DNI / RUC:", cliente.Documento, 85);
        AgregarDato("Teléfono:", cliente.Telefono, 115);

        // CONSULTAR COMPRAS DEL CLIENTE
        var ventas = _ventaService.ListarPorCliente(cliente.Id);

        // Las anuladas se muestran en el historial,
        // pero no cuentan como dinero gastado.
        var ventasValidas = ventas
            .Where(v => !v.Anulada)
            .ToList();

        decimal totalGastado = ventasValidas.Sum(v => v.Total);

        DateTime? ultimaCompra = ventasValidas.Count > 0
            ? ventasValidas.Max(v => v.Fecha)
            : null;

        AgregarDato(
            "Total gastado:",
            $"S/ {totalGastado:N2}",
            155);

        AgregarDato(
            "Última compra:",
            ultimaCompra?.ToString("dd/MM/yyyy HH:mm")
                ?? "Sin compras registradas",
            185);

        // HISTORIAL DE COMPRAS
        var lblHistorial = new Label
        {
            Text = "HISTORIAL DE COMPRAS",
            Location = new Point(20, 230),
            Size = new Size(650, 25),
            Font = new Font("Segoe UI", 12, FontStyle.Bold)
        };
        Controls.Add(lblHistorial);

        var tabla = new DataGridView
        {
            Location = new Point(20, 265),
            Size = new Size(680, 205),
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        tabla.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Venta",
            DataPropertyName = "Id",
            FillWeight = 45
        });

        tabla.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Fecha",
            DataPropertyName = "Fecha",
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "dd/MM/yyyy HH:mm"
            },
            FillWeight = 110
        });

        tabla.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Total (S/)",
            DataPropertyName = "Total",
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleRight
            },
            FillWeight = 80
        });

        tabla.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Estado",
            DataPropertyName = "Estado",
            FillWeight = 70
        });

        tabla.DataSource = ventas;
        Controls.Add(tabla);

        // BOTÓN CERRAR
        var btnCerrar = new Button
        {
            Text = "Cerrar",
            Location = new Point(580, 485),
            Size = new Size(120, 32)
        };

        btnCerrar.Click += (_, _) => Close();
        Controls.Add(btnCerrar);
    }

    // Agrega una fila con etiqueta y valor
    private void AgregarDato(
        string etiqueta,
        string valor,
        int y)
    {
        var lblEtiqueta = new Label
        {
            Text = etiqueta,
            Location = new Point(20, y),
            Size = new Size(135, 25),
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        var lblValor = new Label
        {
            Text = valor,
            Location = new Point(160, y),
            Size = new Size(520, 25),
            Font = new Font("Segoe UI", 10)
        };

        Controls.Add(lblEtiqueta);
        Controls.Add(lblValor);
    }
}
