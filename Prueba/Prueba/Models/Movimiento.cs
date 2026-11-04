using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using System.Globalization;
using System.IO;

namespace Prueba.Models
{
    public class Movimiento
    {
       public int Id { get; set; }
       public int ProductoId { get; set; }
       public string Tipo { get; set; } = string.Empty;
       public int Cantidad { get; set; }
       public string Motivo { get; set; } = string.Empty;
       public DateTime Fecha { get; set; }
            // Propiedades solo para mostrar en pantalla (no se guardan en la base de datos).
       public string CantidadTexto =>
            Tipo == "Entrada" ? $"+{Cantidad}" : $"-{Cantidad}";
       public string MotivoTexto =>
            string.IsNullOrWhiteSpace(Motivo)
                    ? Tipo
                    : $"{Tipo} · {Motivo}";
        public string FechaTexto =>
            Fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("es-MX"));
        public Brush ColorCantidad =>
            new SolidColorBrush(
                Tipo == "Entrada" ? Colors.ForestGreen : Colors.Firebrick);
        
    }
}
