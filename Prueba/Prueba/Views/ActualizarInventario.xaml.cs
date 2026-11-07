using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Prueba.Models;
using Prueba.Services;
using System;
using System.Linq;

namespace Prueba.Views
{
    public sealed partial class ActualizarInventario : Page
    {
        private readonly ProductoService _productoService = new();

        // Producto que está seleccionado (se conserva al recargar la lista).
        private int? _productoId;

        // Evita reaccionar a los cambios de selección hechos por el propio código.
        private bool _cargando;

        public ActualizarInventario()
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
            _cargando = true;

            try
            {
                var productos = _productoService.Buscar(texto);

                ProductosListView.ItemsSource = productos;

                ContadorTextBlock.Text = productos.Count == 1
                    ? "1 producto"
                    : $"{productos.Count} productos";

                // Vuelve a seleccionar el mismo producto, ya con los datos nuevos.
                var actual = productos.FirstOrDefault(p => p.Id == _productoId);
                ProductosListView.SelectedItem = actual;

                MostrarProducto(actual);
            }
            catch (Exception ex)
            {
                ContadorTextBlock.Text =
                    $"No se pudieron cargar los productos: {ex.Message}";
            }
            finally
            {
                _cargando = false;
            }
        }

        private void ProductosListView_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (_cargando)
            {
                return;
            }

            var producto = ProductosListView.SelectedItem as Producto;

            _productoId = producto?.Id;

            // Al elegir otro producto se limpia el formulario.
            CantidadNumberBox.Value = double.NaN;
            MotivoTextBox.Text = string.Empty;
            MensajeTextBlock.Text = string.Empty;

            MostrarProducto(producto);
        }

        private void MostrarProducto(Producto? producto)
        {
            if (producto is null)
            {
                DetalleScrollViewer.Visibility = Visibility.Collapsed;
                SinSeleccionTextBlock.Visibility = Visibility.Visible;
                return;
            }

            SinSeleccionTextBlock.Visibility = Visibility.Collapsed;
            DetalleScrollViewer.Visibility = Visibility.Visible;

            NombreProductoTextBlock.Text = producto.Nombre;
            ExistenciaTextBlock.Text =
                $"Existencias actuales: {producto.ExistenciaTexto}";

            var movimientos = _productoService.ObtenerMovimientos(producto.Id);

            HistorialItemsControl.ItemsSource = movimientos;

            SinMovimientosTextBlock.Visibility = movimientos.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void RegistrarButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ProductosListView.SelectedItem is not Producto producto)
            {
                return;
            }

            if (TipoMovimientoComboBox.SelectedItem is not ComboBoxItem tipoItem)
            {
                MostrarMensaje("Selecciona el tipo de movimiento.", true);
                return;
            }

            string tipo = tipoItem.Tag?.ToString() ?? string.Empty;

            if (double.IsNaN(CantidadNumberBox.Value)
                || CantidadNumberBox.Value < 1)
            {
                MostrarMensaje("Escribe una cantidad de 1 o más piezas.", true);
                return;
            }

            int cantidad = (int)Math.Round(CantidadNumberBox.Value);
            string motivo = MotivoTextBox.Text.Trim();

            try
            {
                _productoService.RegistrarMovimiento(
                    producto.Id, tipo, cantidad, motivo);
            }
            catch (InvalidOperationException ex)
            {
                // Por ejemplo, una salida mayor a las existencias.
                MostrarMensaje(ex.Message, true);
                return;
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    $"No se pudo registrar el movimiento: {ex.Message}", true);
                return;
            }

            CantidadNumberBox.Value = double.NaN;
            MotivoTextBox.Text = string.Empty;

            // Recarga la lista para ver las existencias nuevas y el historial.
            CargarProductos(BusquedaTextBox.Text.Trim());

            MostrarMensaje(
                tipo == "Entrada"
                    ? $"Se registró la entrada de {cantidad} pieza(s)."
                    : $"Se registró la salida de {cantidad} pieza(s).",
                false);
        }

        private void MostrarMensaje(string texto, bool esError)
        {
            MensajeTextBlock.Text = texto;
            MensajeTextBlock.Foreground = new SolidColorBrush(
                esError ? Colors.Firebrick : Colors.ForestGreen);
        }
    }
}