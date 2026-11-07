using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace Prueba.Services
{
    public class UsuarioService
    {
        private readonly string _connectionString;

        public UsuarioService()
        {
            string carpeta = Windows.Storage.ApplicationData
                .Current.LocalFolder.Path;

            string ruta = Path.Combine(carpeta, "inventario.db");
            _connectionString = $"Data Source={ruta}";
            CrearTabla();
        }

        private void CrearTabla()
        {
            using var conexion =
                new SqliteConnection(_connectionString);
            conexion.Open();
            string sql = @"
            CREATE TABLE IF NOT EXISTS Usuarios(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                NombreUsuario TEXT NOT NULL UNIQUE,
                HashContrasena TEXT NOT NULL,
                Rol TEXT NOT NULL,
                Correo TEXT,
                Activo INTEGER NOT NULL DEFAULT 1
            );";
            using var comando = new SqliteCommand(sql, conexion);
            comando.ExecuteNonQuery();
        }

        public bool Registrar(string nombreUsuario, string contrasena)
        {
            const string sql = @"
                INSERT INTO Usuarios
                    (Nombre, NombreUsuario, HashContrasena, Rol, Activo)
                VALUES
                    (@Nombre, @NombreUsuario, @HashContrasena, 'Usuario', 1);";

            try
            {
                using var conexion = new SqliteConnection(_connectionString);
                conexion.Open();

                using var comando = new SqliteCommand(sql, conexion);
                comando.Parameters.AddWithValue("@Nombre", nombreUsuario);
                comando.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                comando.Parameters.AddWithValue("@HashContrasena", contrasena);
                comando.ExecuteNonQuery();
                return true;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return false;
            }
        }

        public bool Validar(string nombreUsuario, string contrasena)
        {
            const string sql = @"
                SELECT HashContrasena
                FROM Usuarios
                WHERE NombreUsuario = @NombreUsuario AND Activo = 1;";

            using var conexion = new SqliteConnection(_connectionString);
            conexion.Open();

            using var comando = new SqliteCommand(sql, conexion);
            comando.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);

            return string.Equals(
                comando.ExecuteScalar()?.ToString(),
                contrasena,
                StringComparison.Ordinal);
        }
    }
}
