using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Prueba.Services;
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
        private readonly UsuarioService _usuarioService;
        public Login()
        {
            InitializeComponent();
            _usuarioService = new UsuarioService();
        }
        private MainWindow? _ventana;

        protected override void OnNavigatedTo(NavigationEventArgs e)
            => _ventana = e.Parameter as MainWindow;

        private void IngresarButton_Click(object sender, RoutedEventArgs e)
        {
            string nombreUsuario = UsuarioTextBox.Text.Trim();
            string contrasena = PasswordBox.Password;

            if ((nombreUsuario == "admin" && contrasena == "admin")
                || _usuarioService.Validar(nombreUsuario, contrasena))
            {
                MensajeError.Text = string.Empty;
                _ventana?.IniciarSesion();
            }
            else
            {
                MensajeError.Text = "Usuario o contraseña incorrectos.";
            }
        }

        private void CrearCuentaButton_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            CrearCuentaPanel.Visibility = Visibility.Visible;
            MensajeRegistro.Text = string.Empty;
        }

        private void CancelarCrearCuentaButton_Click(object sender, RoutedEventArgs e)
        {
            CrearCuentaPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
            LimpiarRegistro();
        }

        private void RegistrarCuentaButton_Click(object sender, RoutedEventArgs e)
        {
            string nombreUsuario = NuevoUsuarioTextBox.Text.Trim();
            string contrasena = NuevaContrasenaPasswordBox.Password;
            string confirmacion = ConfirmarContrasenaPasswordBox.Password;

            if (nombreUsuario.Length < 3)
            {
                MensajeRegistro.Text = "El usuario debe tener al menos 3 caracteres.";
                return;
            }

            if (contrasena.Length < 6)
            {
                MensajeRegistro.Text = "La contraseña debe tener al menos 6 caracteres.";
                return;
            }

            if (contrasena != confirmacion)
            {
                MensajeRegistro.Text = "Las contraseñas no coinciden.";
                return;
            }

            try
            {
                if (!_usuarioService.Registrar(nombreUsuario, contrasena))
                {
                    MensajeRegistro.Text = "Ese usuario ya existe.";
                    return;
                }

                UsuarioTextBox.Text = nombreUsuario;
                PasswordBox.Password = string.Empty;
                LimpiarRegistro();
                CrearCuentaPanel.Visibility = Visibility.Collapsed;
                LoginPanel.Visibility = Visibility.Visible;
                MensajeError.Text = "Cuenta creada. Ya puedes iniciar sesión.";
            }
            catch (Exception ex)
            {
                MensajeRegistro.Text = $"No se pudo crear la cuenta: {ex.Message}";
            }
        }

        private void LimpiarRegistro()
        {
            NuevoUsuarioTextBox.Text = string.Empty;
            NuevaContrasenaPasswordBox.Password = string.Empty;
            ConfirmarContrasenaPasswordBox.Password = string.Empty;
            MensajeRegistro.Text = string.Empty;
        }
    }
}
