using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Prueba.Models;
using Prueba.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Prueba.Views
{
    public sealed partial class Reportes : Page
    {
        private readonly ProductoService _productoService = new();

        private List<Producto> _productos = new();

        // Evita recalcular antes de que la página termine de cargar.
        private bool _listo;

        private static readonly CultureInfo Cultura =
            CultureInfo.GetCultureInfo("es-MX");

        public Reportes()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                CargarDatos();
                _listo = true;
            };
        }

        private void CargarDatos()
        {
            try
            {
                _productos = _productoService.ObtenerTodos();

                TotalProductosTextBlock.Text =
                    _productos.Count.ToString("N0", Cultura);

                TotalPiezasTextBlock.Text =
                    _productos.Sum(p => p.InventarioActual).ToString("N0", Cultura);

                decimal valorTotal =
                    _productos.Sum(p => p.Precio * p.InventarioActual);

                ValorTotalTextBlock.Text = valorTotal.ToString("C2", Cultura);

                ActualizarPocoStock();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    $"No se pudieron cargar los datos: {ex.Message}", true);
            }
        }

        private void UmbralNumberBox_ValueChanged(
            NumberBox sender,
            NumberBoxValueChangedEventArgs args)
        {
            if (!_listo)
            {
                return;
            }

            ActualizarPocoStock();
        }

        private void ActualizarPocoStock()
        {
            int umbral = double.IsNaN(UmbralNumberBox.Value)
                ? 0
                : (int)Math.Round(UmbralNumberBox.Value);

            var pocoStock = _productos
                .Where(p => p.InventarioActual <= umbral)
                .OrderBy(p => p.InventarioActual)
                .ThenBy(p => p.Nombre)
                .ToList();

            PocoStockItemsControl.ItemsSource = pocoStock;

            PocoStockCountTextBlock.Text = pocoStock.Count.ToString("N0", Cultura);

            SinPocoStockTextBlock.Visibility = pocoStock.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private async void ExportarButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (App.Ventana is null)
            {
                return;
            }

            try
            {
                var selector = new FileSavePicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    SuggestedFileName = $"inventario_{DateTime.Now:yyyy-MM-dd}"
                };

                selector.FileTypeChoices.Add(
                    "Archivo CSV (Excel)", new List<string> { ".csv" });

                // En WinUI 3 el selector necesita saber a qué ventana pertenece.
                var ventana = WinRT.Interop.WindowNative.GetWindowHandle(App.Ventana);
                WinRT.Interop.InitializeWithWindow.Initialize(selector, ventana);

                StorageFile? archivo = await selector.PickSaveFileAsync();

                if (archivo is null)
                {
                    return; // La persona canceló.
                }

                // Siempre se exportan los datos más recientes.
                _productos = _productoService.ObtenerTodos();

                var csv = new StringBuilder();

                csv.AppendLine(
                    "Nombre,Marca,Tipo,Categoría,Variante,Importado," +
                    "Inventario inicial,Inventario actual,Precio,Valor en inventario");

                foreach (var p in _productos)
                {
                    csv.AppendLine(string.Join(",",
                        Csv(p.Nombre),
                        Csv(p.Marca),
                        Csv(p.Tipo),
                        Csv(p.Categoria),
                        Csv(p.Variante),
                        p.Importado ? "Sí" : "No",
                        p.InventarioInicial,
                        p.InventarioActual,
                        p.Precio.ToString("0.00", CultureInfo.InvariantCulture),
                        (p.Precio * p.InventarioActual)
                            .ToString("0.00", CultureInfo.InvariantCulture)));
                }

                // Con "BOM" para que Excel muestre bien los acentos y la ñ.
                var codificacion = new UTF8Encoding(true);

                byte[] bytes = codificacion.GetPreamble()
                    .Concat(codificacion.GetBytes(csv.ToString()))
                    .ToArray();

                await FileIO.WriteBytesAsync(archivo, bytes);

                MostrarMensaje(
                    $"Se exportaron {_productos.Count} productos a \"{archivo.Name}\".",
                    false);
            }
            catch (Exception ex)
            {
                MostrarMensaje($"No se pudo exportar: {ex.Message}", true);
            }
        }

        // Pone el texto entre comillas para que las comas no rompan las columnas.
        private static string Csv(string? valor)
        {
            valor ??= string.Empty;

            // Evita que Excel interprete un texto como fórmula (=, +, -, @).
            if (valor.Length > 0 && "=+-@".Contains(valor[0]))
            {
                valor = "'" + valor;
            }

            return "\"" + valor.Replace("\"", "\"\"") + "\"";
        }

        private void MostrarMensaje(string texto, bool esError)
        {
            MensajeTextBlock.Text = texto;
            MensajeTextBlock.Foreground = new SolidColorBrush(
                esError ? Colors.Firebrick : Colors.ForestGreen);
        }
    }
}