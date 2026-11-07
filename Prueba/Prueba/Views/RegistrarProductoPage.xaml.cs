using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Prueba.Models;
using Prueba.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Prueba.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RegistrarProductoPage : Page
    {
        private readonly ProductoService _productoService;
        private string _imagenSeleccionada = string.Empty;

        public RegistrarProductoPage()
        {
            InitializeComponent();

            _productoService = new ProductoService();

        }

        private async void GuardarButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string nombre = NombreTextBox.Text.Trim();
            string marca = MarcaTextBox.Text.Trim();
            string categoria = CategoriaTextBox.Text.Trim();
            string variante = VarianteTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                await MostrarMensaje(
                    "Escribe el nombre del producto.");
                return;
            }

            if (string.IsNullOrWhiteSpace(marca))
            {
                await MostrarMensaje("Escribe la marca del producto.");
                return;
            }

            if (TipoComboBox.SelectedItem
                is not ComboBoxItem tipoSeleccionado)
            {
                await MostrarMensaje(
                    "Selecciona el tipo de producto.");
                return;
            }

            string tipo = tipoSeleccionado.Content?.ToString()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(categoria))
            {
                await MostrarMensaje(
                    "Escribe la categoría o lista.");
                return;
            }

            if (!int.TryParse(
                InventarioTextBox.Text.Trim(),
                out int inventario)
                || inventario < 0)
            {
                await MostrarMensaje(
                    "El inventario debe ser un número entero igual o mayor que cero.");
                return;
            }

            string textoPrecio = PrecioTextBox.Text.Trim();

            bool precioValido = decimal.TryParse(
                textoPrecio,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out decimal precio);

            if (!precioValido)
            {
                // Permite también escribir decimales con punto.
                precioValido = decimal.TryParse(
                    textoPrecio,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out precio);
            }

            if (!precioValido || precio < 0)
            {
                await MostrarMensaje(
                    "Escribe un precio válido igual o mayor que cero.");
                return;
            }

            var producto = new Producto
            {
                Nombre = nombre,
                Marca = marca,
                Imagen = _imagenSeleccionada,
                Tipo = tipo,
                Categoria = categoria,
                Variante = variante,
                Importado = ImportadoCheckBox.IsChecked == true,
                InventarioInicial = inventario,
                InventarioActual = inventario,
                Precio = precio,
            };

            try
            {
                if (_productoService.ExisteProducto(nombre, marca, variante))
                {
                    bool guardarIgual = await Confirmar(
                        "Producto repetido",
                        "Ya existe un producto con el mismo nombre, marca y variante.\n" +
                        "¿Quieres guardarlo de todos modos?");

                    if (!guardarIgual)
                    {
                        return;
                    }
                }

                int id = _productoService.GuardarProducto(producto);

                await MostrarMensaje(
                    $"Producto guardado correctamente.\nID asignado: {id}");

                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                await MostrarMensaje(
                    $"No se pudo guardar el producto.\n{ex.Message}");
            }
        }

        private async void SeleccionarImagenButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (App.Ventana is null)
            {
                return;
            }

            var selector = new FileOpenPicker();
            selector.FileTypeFilter.Add(".jpg");
            selector.FileTypeFilter.Add(".jpeg");
            selector.FileTypeFilter.Add(".png");
            selector.FileTypeFilter.Add(".webp");

            IntPtr ventana = WindowNative.GetWindowHandle(App.Ventana);
            InitializeWithWindow.Initialize(selector, ventana);

            var archivo = await selector.PickSingleFileAsync();
            if (archivo is null)
            {
                return;
            }

            try
            {
                // Copia la imagen a la carpeta de datos de la app, para que
                // el producto no la pierda si el archivo original se mueve o se borra.
                var carpetaImagenes = await Windows.Storage.ApplicationData.Current
                    .LocalFolder.CreateFolderAsync(
                        "Imagenes",
                        Windows.Storage.CreationCollisionOption.OpenIfExists);

                string nombreUnico =
                    $"{Guid.NewGuid():N}{archivo.FileType.ToLowerInvariant()}";

                var copia = await archivo.CopyAsync(
                    carpetaImagenes,
                    nombreUnico,
                    Windows.Storage.NameCollisionOption.GenerateUniqueName);

                // Si ya había elegido otra imagen sin guardar, borra esa copia.
                BorrarCopiaAnterior();

                _imagenSeleccionada = copia.Path;
            }
            catch (Exception ex)
            {
                await MostrarMensaje(
                    $"No se pudo cargar la imagen.\n{ex.Message}");
                return;
            }

            ImagenSeleccionadaTextBlock.Text = archivo.Name;
            ImagenProducto.Source = new BitmapImage(new Uri(_imagenSeleccionada));
            ImagenProducto.Visibility = Visibility.Visible;
        }

        private void BorrarCopiaAnterior()
        {
            if (string.IsNullOrEmpty(_imagenSeleccionada))
            {
                return;
            }

            try
            {
                File.Delete(_imagenSeleccionada);
            }
            catch
            {
                // Si no se puede borrar, no pasa nada: solo queda un archivo de sobra.
            }
        }

        private async System.Threading.Tasks.Task<bool> Confirmar(
            string titulo,
            string mensaje)
        {
            var dialogo = new ContentDialog
            {
                Title = titulo,
                Content = mensaje,
                PrimaryButtonText = "Guardar de todos modos",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            return await dialogo.ShowAsync() == ContentDialogResult.Primary;
        }

        private async System.Threading.Tasks.Task MostrarMensaje(
            string mensaje)
        {
            var dialogo = new ContentDialog
            {
                Title = "Fiesta",
                Content = mensaje,
                CloseButtonText = "Aceptar",
                XamlRoot = this.XamlRoot
            };

            await dialogo.ShowAsync();
        }

        private void LimpiarFormulario()
        {
            NombreTextBox.Text = string.Empty;
            MarcaTextBox.Text = string.Empty;
            TipoComboBox.SelectedIndex = -1;
            CategoriaTextBox.Text = string.Empty;
            VarianteTextBox.Text = string.Empty;
            ImportadoCheckBox.IsChecked = false;
            InventarioTextBox.Text = string.Empty;
            PrecioTextBox.Text = string.Empty;
            _imagenSeleccionada = string.Empty;
            ImagenSeleccionadaTextBlock.Text = "Ninguna imagen seleccionada";
            ImagenProducto.Source = null;
            ImagenProducto.Visibility = Visibility.Collapsed;
        }

    }
}