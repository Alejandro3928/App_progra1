
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml;
using Prueba.Models;
using System;
using System.IO;

namespace Prueba.Services
{
    public class ProductoService
    {
        private readonly string _connectionString;

        public ProductoService()
        {
            string carpeta = Windows.Storage.ApplicationData
                .Current.LocalFolder.Path;

            string ruta = Path.Combine(carpeta, "inventario.db");

            _connectionString =
                $"Data Source={ruta}";

            CrearTabla();
        }

        private void CrearTabla()
        {
            using var conexion =
                new SqliteConnection(_connectionString);

            conexion.Open();

            string sql = @"
                CREATE TABLE IF NOT EXISTS Productos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre TEXT NOT NULL,
                    Tipo TEXT NOT NULL,
                    Categoria TEXT NOT NULL,
                    Variante TEXT NOT NULL,
                    Importado INTEGER NOT NULL,
                    InventarioInicial INTEGER NOT NULL,
                    InventarioActual INTEGER NOT NULL,
                    Precio REAL NOT NULL
                );";

            using var comando =
                new SqliteCommand(sql, conexion);

            comando.ExecuteNonQuery();
        }

        public int GuardarProducto(Producto producto)
        {
            using var conexion =
                new SqliteConnection(_connectionString);

            conexion.Open();

            string sql = @"
                INSERT INTO Productos
                (
                    Nombre,
                    Tipo,
                    Categoria,
                    Variante,
                    Importado,
                    InventarioInicial,
                    InventarioActual,
                    Precio
                )
                VALUES
                (
                    @Nombre,
                    @Tipo,
                    @Categoria,
                    @Variante,
                    @Importado,
                    @InventarioInicial,
                    @InventarioActual,
                    @Precio
                );";

            using var comando =
                new SqliteCommand(sql, conexion);

            comando.Parameters.AddWithValue(
                "@Nombre", producto.Nombre);

            comando.Parameters.AddWithValue(
                "@Tipo", producto.Tipo);

            comando.Parameters.AddWithValue(
                "@Categoria", producto.Categoria);

            comando.Parameters.AddWithValue(
                "@Variante", producto.Variante);

            comando.Parameters.AddWithValue(
                "@Importado", producto.Importado ? 1 : 0);

            comando.Parameters.AddWithValue(
                "@InventarioInicial", producto.InventarioInicial);

            comando.Parameters.AddWithValue(
                "@InventarioActual", producto.InventarioInicial);

            comando.Parameters.AddWithValue(
                "@Precio", producto.Precio);

            comando.ExecuteNonQuery();

            using var idComando =
                new SqliteCommand(
                    "SELECT last_insert_rowid();", conexion);

            return Convert.ToInt32(
                idComando.ExecuteScalar());
        }
    }
}
