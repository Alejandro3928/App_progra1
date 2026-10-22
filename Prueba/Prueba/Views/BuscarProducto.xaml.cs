using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Prueba.Services;
using System;

namespace Prueba.Views
{
    public sealed partial class BuscarProducto : Page
    {
        private readonly ProductoService _productoService = new();

        public BuscarProducto()
        {
            InitializeComponent();

            // Carga la lista completa al abrir la página.
            Loaded += (s, e) => CargarProductos(string.Empty);
        }

        private void BusquedaTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            CargarProductos(BusquedaTextBox.Text.Trim());
        }

        private void CargarProductos(string texto)
        {
            try
            {
                var productos = _productoService.Buscar(texto);

                ProductosListView.ItemsSource = productos;

                ContadorTextBlock.Text = productos.Count == 1
                    ? "1 producto"
                    : $"{productos.Count} productos";

                SinResultadosTextBlock.Visibility = productos.Count == 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                ContadorTextBlock.Text =
                    $"No se pudieron cargar los productos: {ex.Message}";
            }
        }
    }
}