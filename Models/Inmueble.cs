using System.ComponentModel.DataAnnotations;

namespace Practica2.Models
{
    public class Inmueble
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Codigo { get; set; } // [único]

        [Required]
        public string Titulo { get; set; }

        public string Imagen { get; set; }

        [Required]
        public TipoInmueble Tipo { get; set; }

        [Required]
        public string Ciudad { get; set; }

        [Required]
        public string Direccion { get; set; }

        public int Dormitorios { get; set; }
        public int Banos { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Los metros cuadrados deben ser mayores que 0.")]
        public int MetrosCuadrados { get; set; } // [Restricción: > 0]

        [Range(1, double.MaxValue, ErrorMessage = "El precio debe ser mayor que 0.")]
        [DataType(DataType.Currency)]
        public decimal Precio { get; set; } // [Restricción: > 0]

        public bool Activo { get; set; }
    }
}