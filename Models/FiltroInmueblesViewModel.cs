using System.ComponentModel.DataAnnotations;

namespace Practica2.Models
{
    public class FiltroInmueblesViewModel : IValidatableObject
    {
        public string Ciudad { get; set; }
        public TipoInmueble? Tipo { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio mínimo no puede ser negativo.")]
        public decimal? PrecioMin { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio máximo no puede ser negativo.")]
        public decimal? PrecioMax { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad de dormitorios no puede ser negativa.")]
        public int? Dormitorios { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            if (PrecioMin.HasValue && PrecioMax.HasValue && PrecioMin > PrecioMax)
            {
                yield return new ValidationResult(
                    "El precio mínimo no puede ser mayor que el precio máximo.",
                    new[] { nameof(PrecioMax) } 
                );
            }
        }
    }
}