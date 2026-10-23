using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Prueba.Services;
using Prueba.Models;
using System;

namespace Prueba.Views
{
    public sealed partial class BuscarProducto : Page
    {
        private readonly ProductoService _productoService = new();

        public BuscarProducto()
        {
            InitializeComponent();

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

        // BOTÓN ELIMINAR
        private async void EliminarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button boton || boton.DataContext is not Producto producto)
            {
                return;
            }

            var dialogo = new ContentDialog
            {
                Title = "Eliminar producto",
                Content = $"¿Estás segura de que deseas eliminar '{producto.Nombre}'?",
                PrimaryButtonText = "Eliminar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var resultado = await dialogo.ShowAsync();

            if (resultado != ContentDialogResult.Primary)
            {
                return;
            }

            try
            {
                bool eliminado = _productoService.EliminarProducto(producto.Id);

                CargarProductos(BusquedaTextBox.Text.Trim());

                if (eliminado)
                {
                    var mensaje = new ContentDialog
                    {
                        Title = "Producto eliminado",
                        Content = "El producto se eliminó correctamente.",
                        CloseButtonText = "Aceptar",
                        XamlRoot = this.XamlRoot
                    };

                    await mensaje.ShowAsync();
                }
                else
                {
                    var mensaje = new ContentDialog
                    {
                        Title = "Aviso",
                        Content = "No se encontró el producto.",
                        CloseButtonText = "Aceptar",
                        XamlRoot = this.XamlRoot
                    };

                    await mensaje.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var mensaje = new ContentDialog
                {
                    Title = "Error",
                    Content = $"No se pudo eliminar el producto.\n{ex.Message}",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.XamlRoot
                };

                await mensaje.ShowAsync();
            }
        }

        // BOTÓN EDITAR
        private async void EditarProducto_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button boton ||
                boton.DataContext is not Producto producto)
            {
                return;
            }

            var mensaje = new ContentDialog
            {
                Title = "Editar producto",
                Content = $"Seleccionaste: {producto.Nombre}\nID: {producto.Id}",
                CloseButtonText = "Aceptar",
                XamlRoot = this.XamlRoot
            };

            await mensaje.ShowAsync();
        }
    }
}