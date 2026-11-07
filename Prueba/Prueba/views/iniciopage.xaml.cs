using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Prueba.Services;
using System;
using System.Globalization;
using System.Linq;

namespace Prueba.Views
{
    public sealed partial class InicioPage : Page
    {
        // Mismo límite que trae por defecto la página de Reportes.
        private const int LimitePocoStock = 5;

        private readonly ProductoService _productoService = new();

        public InicioPage()
        {
            InitializeComponent();

            Loaded += (s, e) => CargarResumen();
        }

        private void CargarResumen()
        {
            try
            {
                var productos = _productoService.ObtenerTodos();

                bool hayProductos = productos.Count > 0;

                ResumenPanel.Visibility = hayProductos
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                PrimerosPasosBorder.Visibility = hayProductos
                    ? Visibility.Collapsed
                    : Visibility.Visible;

                if (!hayProductos)
                {
                    return;
                }

                var cultura = CultureInfo.GetCultureInfo("es-MX");

                TotalProductosTextBlock.Text =
                    productos.Count.ToString("N0", cultura);

                TotalPiezasTextBlock.Text =
                    productos.Sum(p => p.InventarioActual).ToString("N0", cultura);

                var pocoStock = productos
                    .Where(p => p.InventarioActual <= LimitePocoStock)
                    .OrderBy(p => p.InventarioActual)
                    .ThenBy(p => p.Nombre)
                    .ToList();

                PocoStockCountTextBlock.Text =
                    pocoStock.Count.ToString("N0", cultura);

                // Muestra solo los 5 más urgentes.
                ReponerItemsControl.ItemsSource = pocoStock.Take(5).ToList();

                ReponerBorder.Visibility = pocoStock.Count == 0
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
            catch (Exception)
            {
                // Si no se pueden leer los datos, se deja la pantalla de primeros pasos.
                ResumenPanel.Visibility = Visibility.Collapsed;
                PrimerosPasosBorder.Visibility = Visibility.Visible;
            }
        }

        private void RegistrarProductoBorder_Tapped(
            object sender,
            TappedRoutedEventArgs e)
        {
            App.Ventana?.IrA("Registrar");
        }

        private void ExistenciasCard_Tapped(
            object sender,
            TappedRoutedEventArgs e)
        {
            App.Ventana?.IrA("Buscar");
        }

        private void ReponerBorder_Tapped(
            object sender,
            TappedRoutedEventArgs e)
        {
            App.Ventana?.IrA("Reportes");
        }
    }
}