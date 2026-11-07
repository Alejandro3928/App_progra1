using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Prueba.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Prueba
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainFrame.Navigate(typeof(Login));
        }

        private void MenuPrincipal_SelectionChanged(
            NavigationView sender,
            NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem
                is not NavigationViewItem item)
            {
                return;
            }

            switch (item.Tag?.ToString())
            {
                case "Inicio":
                    MainFrame.Navigate(
                        typeof(InicioPage));
                    break;

                case "Registrar":
                    MainFrame.Navigate(
                        typeof(RegistrarProductoPage));
                    break;

                case "Buscar":
                    MainFrame.Navigate(
                        typeof(InicioPage));
                    break;

                case "Actualizar":
                    MainFrame.Navigate(
                        typeof(InicioPage));
                    break;

                case "Reportes":
                    MainFrame.Navigate(
                        typeof(InicioPage));
                    break;
            }
        }
    }
}
