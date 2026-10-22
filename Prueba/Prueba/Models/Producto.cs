using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
    }
}

