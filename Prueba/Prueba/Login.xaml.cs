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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Prueba
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
        }
        private void IngresarButton_Click(object sender, RoutedEventArgs e)
        {
            string usuario = UsuarioTextBox.Text;
            string contraseña = PasswordBox.Password;

            if (usuario == "admin" && contraseña == "1234")
            {
                MensajeError.Text = "¡Inicio de sesión correcto!";
            }
            else
            {
                // Mostrar mensaje de error
                MensajeError.Text = "Usuario o contraseña incorrectos.";
            }
        }
    }
}
