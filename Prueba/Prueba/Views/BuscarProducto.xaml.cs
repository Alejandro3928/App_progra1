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

            var nombreBox = new TextBox
            {
                Header = "Nombre",
                Text = producto.Nombre
            };

            var marcaBox = new TextBox
            {
                Header = "Marca",
                Text = producto.Marca
            };

            string[] tipos =
            {
                "Decoración", "Desechables", "Piñatas",
                "Dulces y regalos", "Otros"
            };

            var tipoCombo = new ComboBox
            {
                Header = "Tipo",
                ItemsSource = tipos,
                SelectedItem = producto.Tipo,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var categoriaBox = new TextBox
            {
                Header = "Categoría o lista",
                Text = producto.Categoria
            };

            var varianteBox = new TextBox
            {
                Header = "Variante (opcional)",
                Text = producto.Variante
            };

            var importadoCheck = new CheckBox
            {
                Content = "Es importado",
                IsChecked = producto.Importado
            };

            var inventarioBox = new NumberBox
            {
                Header = "Inventario actual",
                Value = producto.InventarioActual,
                Minimum = 0,
                SmallChange = 1,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
            };

            var precioBox = new NumberBox
            {
                Header = "Precio",
                Value = (double)producto.Precio,
                Minimum = 0,
                SmallChange = 1,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
            };

            var errorTexto = new TextBlock
            {
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.Red),
                TextWrapping = TextWrapping.Wrap
            };

            var panel = new StackPanel { Spacing = 12 };
            panel.Children.Add(nombreBox);
            panel.Children.Add(marcaBox);
            panel.Children.Add(tipoCombo);
            panel.Children.Add(categoriaBox);
            panel.Children.Add(varianteBox);
            panel.Children.Add(importadoCheck);
            panel.Children.Add(inventarioBox);
            panel.Children.Add(precioBox);
            panel.Children.Add(errorTexto);

            var dialogo = new ContentDialog
            {
                Title = "Editar producto",
                Content = new ScrollViewer { Content = panel },
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            // Valida antes de cerrar: si hay un error, el cuadro se queda abierto.
            dialogo.PrimaryButtonClick += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(nombreBox.Text))
                {
                    errorTexto.Text = "Escribe el nombre del producto.";
                    args.Cancel = true;
                }
                else if (string.IsNullOrWhiteSpace(marcaBox.Text))
                {
                    errorTexto.Text = "Escribe la marca del producto.";
                    args.Cancel = true;
                }
                else if (tipoCombo.SelectedItem is not string)
                {
                    errorTexto.Text = "Selecciona el tipo de producto.";
                    args.Cancel = true;
                }
                else if (string.IsNullOrWhiteSpace(categoriaBox.Text))
                {
                    errorTexto.Text = "Escribe la categoría o lista.";
                    args.Cancel = true;
                }
                else if (double.IsNaN(inventarioBox.Value)
                         || inventarioBox.Value < 0)
                {
                    errorTexto.Text = "El inventario debe ser un número igual o mayor que cero.";
                    args.Cancel = true;
                }
                else if (double.IsNaN(precioBox.Value)
                         || precioBox.Value < 0)
                {
                    errorTexto.Text = "Escribe un precio válido igual o mayor que cero.";
                    args.Cancel = true;
                }
            };

            var resultado = await dialogo.ShowAsync();

            if (resultado != ContentDialogResult.Primary)
            {
                return;
            }

            // Pasa los valores nuevos al producto (la imagen y el inventario
            // inicial se conservan como estaban).
            producto.Nombre = nombreBox.Text.Trim();
            producto.Marca = marcaBox.Text.Trim();
            producto.Tipo = (string)tipoCombo.SelectedItem;
            producto.Categoria = categoriaBox.Text.Trim();
            producto.Variante = varianteBox.Text.Trim();
            producto.Importado = importadoCheck.IsChecked == true;
            producto.InventarioActual = (int)Math.Round(inventarioBox.Value);
            producto.Precio = Math.Round((decimal)precioBox.Value, 2);

            try
            {
                _productoService.ActualizarProducto(producto);
            }
            catch (Exception ex)
            {
                var error = new ContentDialog
                {
                    Title = "Error",
                    Content = $"No se pudo guardar el producto.\n{ex.Message}",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.XamlRoot
                };

                await error.ShowAsync();
            }

            // Vuelve a leer la lista desde la base de datos para mostrar los cambios.
            CargarProductos(BusquedaTextBox.Text.Trim());

        }
    }
}