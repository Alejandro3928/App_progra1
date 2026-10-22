using System.Globalization;

namespace Prueba.Models
{
    public class Producto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Tipo { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public string Variante { get; set; } = string.Empty;

        public bool Importado { get; set; }

        public int InventarioInicial { get; set; }

        public int InventarioActual { get; set; }

        public decimal Precio { get; set; }

        // Propiedades solo para mostrar en pantalla (no se guardan en la base de datos).

        public string PrecioTexto =>
            Precio.ToString("C2", CultureInfo.GetCultureInfo("es-MX"));

        public string ExistenciaTexto =>
            InventarioActual == 1 ? "1 pieza" : $"{InventarioActual} piezas";

        public string ImportadoTexto =>
            Importado ? "Importado" : "Nacional";

        public string Detalle =>
            string.IsNullOrWhiteSpace(Variante)
                ? $"{Tipo} · {Categoria}"
                : $"{Tipo} · {Categoria} · {Variante}";
    }
}
