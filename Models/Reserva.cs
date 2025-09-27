
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Practica2.Models
{
    public class Reserva
    {
        public int Id { get; set; }

      
        public int InmuebleId { get; set; }
        public string UsuarioId { get; set; } 
        [DataType(DataType.DateTime)]
        public DateTime FechaExpiracion { get; set; } 

        [DataType(DataType.DateTime)]
        public DateTime FechaCreacion { get; set; }

        
        public Inmueble Inmueble { get; set; }
        public IdentityUser Usuario { get; set; } 
    }
}