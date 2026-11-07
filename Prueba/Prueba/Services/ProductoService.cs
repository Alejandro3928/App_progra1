using Microsoft.Data.Sqlite;
using Prueba.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            _connectionString = $"Data Source={ruta}";
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
                    Marca TEXT NOT NULL DEFAULT '',
                    Imagen TEXT NOT NULL DEFAULT '',
                    Importado INTEGER NOT NULL,
                    InventarioInicial INTEGER NOT NULL,
                    InventarioActual INTEGER NOT NULL,
                    Precio REAL NOT NULL
                );";

            using var comando =
                new SqliteCommand(sql, conexion);

            comando.ExecuteNonQuery();

            string sqlMovimientos = @"
                CREATE TABLE IF NOT EXISTS Movimientos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductoId INTEGER NOT NULL,
                    Tipo TEXT NOT NULL,
                    Cantidad INTEGER NOT NULL,
                    Motivo TEXT NOT NULL DEFAULT '',
                    Fecha TEXT NOT NULL
                );";

            using var comandoMovimientos =
                new SqliteCommand(sqlMovimientos, conexion);

            comandoMovimientos.ExecuteNonQuery();

            AgregarColumnaSiNoExiste(conexion, "Marca");
            AgregarColumnaSiNoExiste(conexion, "Imagen");
        }

        private static void AgregarColumnaSiNoExiste(
            SqliteConnection conexion,
            string columna)
        {
            try
            {
                using var comando = new SqliteCommand(
                    $"ALTER TABLE Productos ADD COLUMN {columna} TEXT NOT NULL DEFAULT '';",
                    conexion);
                comando.ExecuteNonQuery();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 1)
            {
            }
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
                    Marca,
                    Imagen,
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
                    @Marca,
                    @Imagen,
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
                "@Marca", producto.Marca);

            comando.Parameters.AddWithValue(
                "@Imagen", producto.Imagen);

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

        public bool EliminarProducto(int id)
        {
            using var conexion = new SqliteConnection(_connectionString);
            conexion.Open();

            using var transaccion = conexion.BeginTransaction();

            using (var borrarMovimientos = new SqliteCommand(
                "DELETE FROM Movimientos WHERE ProductoId = @Id;",
                conexion,
                transaccion))
            {
                borrarMovimientos.Parameters.AddWithValue("@Id", id);
                borrarMovimientos.ExecuteNonQuery();
            }

            int filasAfectadas;

            using (var borrarProducto = new SqliteCommand(
                "DELETE FROM Productos WHERE Id = @Id;",
                conexion,
                transaccion))
            {
                borrarProducto.Parameters.AddWithValue("@Id", id);
                filasAfectadas = borrarProducto.ExecuteNonQuery();
            }

            transaccion.Commit();
            return filasAfectadas > 0;
        }

        public void ActualizarProducto(Producto producto)
        {
            using var conexion =
                new SqliteConnection(_connectionString);
            conexion.Open();
            string sql = @"
                UPDATE Productos
                SET Nombre = @Nombre,
                    Tipo = @Tipo,
                    Categoria = @Categoria,
                    Variante = @Variante,
                    Marca = @Marca,
                    Imagen = @Imagen,
                    Importado = @Importado,
                    InventarioInicial = @InventarioInicial,
                    InventarioActual = @InventarioActual,
                    Precio = @Precio
                WHERE Id = @Id;";
            using var comando =
                new SqliteCommand(sql, conexion);
            comando.Parameters.AddWithValue(
                "@Id", producto.Id);
            comando.Parameters.AddWithValue(
                "@Nombre", producto.Nombre);
            comando.Parameters.AddWithValue(
                "@Tipo", producto.Tipo);
            comando.Parameters.AddWithValue(
                "@Categoria", producto.Categoria);
            comando.Parameters.AddWithValue(
                "@Variante", producto.Variante);
            comando.Parameters.AddWithValue(
                "@Marca", producto.Marca);
            comando.Parameters.AddWithValue(
                "@Imagen", producto.Imagen);
            comando.Parameters.AddWithValue(
                "@Importado", producto.Importado ? 1 : 0);
            comando.Parameters.AddWithValue(
                "@InventarioInicial", producto.InventarioInicial);
            comando.Parameters.AddWithValue(
                "@InventarioActual", producto.InventarioActual);
            comando.Parameters.AddWithValue(
                "@Precio", producto.Precio);
            comando.ExecuteNonQuery();
        }

        /// <summary>
        /// Registra una entrada o una salida de piezas. Actualiza las
        /// existencias y guarda el movimiento en una sola operación:
        /// si algo falla, no se guarda ninguna de las dos cosas.
        /// </summary>
        public void RegistrarMovimiento(
            int productoId,
            string tipo,
            int cantidad,
            string motivo)
        {
            if (tipo != "Entrada" && tipo != "Salida")
            {
                throw new ArgumentException("Tipo de movimiento no válido.");
            }

            if (cantidad <= 0)
            {
                throw new ArgumentException(
                    "La cantidad debe ser mayor que cero.");
            }

            int cambio = tipo == "Entrada" ? cantidad : -cantidad;

            using var conexion =
                new SqliteConnection(_connectionString);

            conexion.Open();

            using var transaccion = conexion.BeginTransaction();

            // La condición "InventarioActual + @Cambio >= 0" evita
            // que una salida deje las existencias en negativo.
            const string sqlActualizar = @"
                UPDATE Productos
                SET InventarioActual = InventarioActual + @Cambio
                WHERE Id = @Id
                  AND InventarioActual + @Cambio >= 0;";

            using (var actualizar = new SqliteCommand(
                sqlActualizar, conexion, transaccion))
            {
                actualizar.Parameters.AddWithValue("@Cambio", cambio);
                actualizar.Parameters.AddWithValue("@Id", productoId);

                if (actualizar.ExecuteNonQuery() == 0)
                {
                    throw new InvalidOperationException(
                        "No hay suficientes existencias para esta salida " +
                        "(o el producto ya no existe).");
                }
            }

            const string sqlMovimiento = @"
                INSERT INTO Movimientos
                    (ProductoId, Tipo, Cantidad, Motivo, Fecha)
                VALUES
                    (@ProductoId, @Tipo, @Cantidad, @Motivo, @Fecha);";

            using (var insertar = new SqliteCommand(
                sqlMovimiento, conexion, transaccion))
            {
                insertar.Parameters.AddWithValue("@ProductoId", productoId);
                insertar.Parameters.AddWithValue("@Tipo", tipo);
                insertar.Parameters.AddWithValue("@Cantidad", cantidad);
                insertar.Parameters.AddWithValue("@Motivo", motivo);
                insertar.Parameters.AddWithValue(
                    "@Fecha",
                    DateTime.Now.ToString(
                        "yyyy-MM-dd HH:mm:ss",
                        CultureInfo.InvariantCulture));

                insertar.ExecuteNonQuery();
            }

            transaccion.Commit();
        }

        /// <summary>
        /// Devuelve los últimos movimientos de un producto, del más
        /// reciente al más antiguo.
        /// </summary>
        public List<Movimiento> ObtenerMovimientos(
            int productoId,
            int maximo = 30)
        {
            var movimientos = new List<Movimiento>();

            using var conexion =
                new SqliteConnection(_connectionString);

            conexion.Open();

            const string sql = @"
                SELECT Id, ProductoId, Tipo, Cantidad, Motivo, Fecha
                FROM Movimientos
                WHERE ProductoId = @ProductoId
                ORDER BY Fecha DESC, Id DESC
                LIMIT @Maximo;";

            using var comando = new SqliteCommand(sql, conexion);
            comando.Parameters.AddWithValue("@ProductoId", productoId);
            comando.Parameters.AddWithValue("@Maximo", maximo);

            using var lector = comando.ExecuteReader();

            while (lector.Read())
            {
                movimientos.Add(new Movimiento
                {
                    Id = lector.GetInt32(0),
                    ProductoId = lector.GetInt32(1),
                    Tipo = lector.GetString(2),
                    Cantidad = lector.GetInt32(3),
                    Motivo = lector.GetString(4),
                    Fecha = DateTime.ParseExact(
                        lector.GetString(5),
                        "yyyy-MM-dd HH:mm:ss",
                        CultureInfo.InvariantCulture)
                });
            }

            return movimientos;
        }

        /// <summary>
        /// Devuelve todos los productos, ordenados por nombre.
        /// </summary>
        public List<Producto> ObtenerTodos()
        {
            return Buscar(string.Empty);
        }

        /// <summary>
        /// Busca productos cuyo nombre, tipo, categoría o variante
        /// contengan el texto indicado. Con texto vacío devuelve todos.
        /// </summary>
        public List<Producto> Buscar(string texto)
        {
            var productos = new List<Producto>();

            using var conexion =
                new SqliteConnection(_connectionString);

            conexion.Open();

            string sql = @"
                SELECT
                    Id,
                    Nombre,
                    Tipo,
                    Categoria,
                    Variante,
                    Marca,
                    Imagen,
                    Importado,
                    InventarioInicial,
                    InventarioActual,
                    Precio
                FROM Productos
                WHERE Nombre    LIKE @Texto
                   OR Tipo      LIKE @Texto
                   OR Categoria LIKE @Texto
                   OR Variante  LIKE @Texto
                   OR Marca     LIKE @Texto
                ORDER BY Nombre;";

            using var comando =
                new SqliteCommand(sql, conexion);

            comando.Parameters.AddWithValue(
                "@Texto", $"%{texto}%");

            using var lector = comando.ExecuteReader();

            while (lector.Read())
            {
                productos.Add(new Producto
                {
                    Id = lector.GetInt32(0),
                    Nombre = lector.GetString(1),
                    Tipo = lector.GetString(2),
                    Categoria = lector.GetString(3),
                    Variante = lector.GetString(4),
                    Marca = lector.GetString(5),
                    Imagen = lector.GetString(6),
                    Importado = lector.GetInt32(7) == 1,
                    InventarioInicial = lector.GetInt32(8),
                    InventarioActual = lector.GetInt32(9),
                    Precio = Convert.ToDecimal(lector.GetDouble(10))
                });
            }

            return productos;

        }

        /// <summary>
        /// Indica si ya existe un producto con el mismo nombre, marca y variante.
        /// </summary>
        public bool ExisteProducto(string nombre, string marca, string variante)
        {
            using var conexion =
                new SqliteConnection(_connectionString);

            conexion.Open();

            const string sql = @"
        SELECT COUNT(*)
        FROM Productos
        WHERE Nombre   = @Nombre   COLLATE NOCASE
          AND Marca    = @Marca    COLLATE NOCASE
          AND Variante = @Variante COLLATE NOCASE;";

            using var comando = new SqliteCommand(sql, conexion);
            comando.Parameters.AddWithValue("@Nombre", nombre);
            comando.Parameters.AddWithValue("@Marca", marca);
            comando.Parameters.AddWithValue("@Variante", variante);

            return Convert.ToInt32(comando.ExecuteScalar()) > 0;
        }
    }
}