using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Practica2.Models
{
    public class Visita
    {
        public int Id { get; set; }
        public int InmuebleId { get; set; }
        public string UsuarioId { get; set; } 

        [Required(ErrorMessage = "La fecha de inicio es requerida.")]
        [DataType(DataType.DateTime)]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es requerida.")]
        [DataType(DataType.DateTime)]
        public DateTime FechaFin { get; set; }

        public EstadoVisita Estado { get; set; }

        [StringLength(500)]
        public string Notas { get; set; }
        public Inmueble Inmueble { get; set; }
        public IdentityUser Usuario { get; set; }
    }
}