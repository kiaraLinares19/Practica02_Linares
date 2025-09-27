using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Practica2.Data;
using Practica2.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; 

[cite_start]
[Authorize] 
public class VisitasController : Controller
{
    private readonly ApplicationDbContext _context;

    public VisitasController(ApplicationDbContext context)
    {
        _context = context;
    }

    
    [HttpGet]
    public async Task<IActionResult> Agendar(int inmuebleId)
    {
        var inmueble = await _context.Inmuebles.FindAsync(inmuebleId);
        if (inmueble == null) return NotFound();

       
        var model = new Visita { InmuebleId = inmuebleId, Inmueble = inmueble };
        return View(model);
    }

   
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Agendar(Visita visita)
    {
       
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        visita.UsuarioId = userId;
        visita.Estado = EstadoVisita.Solicitada; 

        [cite_start]
        if (visita.FechaInicio >= visita.FechaFin)
        {
            ModelState.AddModelError(nameof(visita.FechaFin), "La fecha de fin debe ser posterior a la fecha de inicio.");
        }
        if (visita.FechaInicio < DateTime.Now)
        {
            ModelState.AddModelError(nameof(visita.FechaInicio), "No puedes agendar una visita en el pasado.");
        }
        
        [cite_start]
        if (visita.FechaInicio.Hour < 8 || visita.FechaFin.Hour > 19)
        {
            ModelState.AddModelError(string.Empty, "Las visitas deben estar dentro del horario laboral (08:00 - 19:00).");
        }
        
        [cite_start]
        bool isOverlapping = await _context.Visitas
            .AnyAsync(v => 
                v.InmuebleId == visita.InmuebleId &&
                v.Estado != EstadoVisita.Cancelada && // Solo considerar visitas no canceladas
                (
                    (visita.FechaInicio < v.FechaFin && visita.FechaFin > v.FechaInicio)
                )
            );

        if (isOverlapping)
        {
            ModelState.AddModelError(string.Empty, "¡Error! Ya existe una visita agendada para este inmueble que se solapa con el horario seleccionado.");
        }
        
        [cite_start]
        if (!ModelState.IsValid)
        {
            // Necesario para que la vista pueda mostrar el título del inmueble
            visita.Inmueble = await _context.Inmuebles.FindAsync(visita.InmuebleId);
            return View(visita);
        }

     
        _context.Add(visita);
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "Visita agendada exitosamente. Espera la confirmación del broker.";
        return RedirectToAction("Detalle", "Inmuebles", new { id = visita.InmuebleId });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reservar(int inmuebleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        [cite_start]
        var reservaActiva = await _context.Reservas
            .FirstOrDefaultAsync(r => 
                r.InmuebleId == inmuebleId && 
                r.FechaExpiracion > DateTime.Now);

        if (reservaActiva != null)
        {
            TempData["ErrorMessage"] = "Este inmueble ya tiene una reserva activa.";
            return RedirectToAction("Detalle", "Inmuebles", new { id = inmuebleId });
        }

        [cite_start]
        var nuevaReserva = new Reserva
        {
            InmuebleId = inmuebleId,
            UsuarioId = userId,
            FechaCreacion = DateTime.Now,
            FechaExpiracion = DateTime.Now.AddHours(48) // Bloqueo de 48 horas
        };

        _context.Add(nuevaReserva);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Inmueble reservado por 48 horas. Expira el {nuevaReserva.FechaExpiracion:G}.";
        return RedirectToAction("Detalle", "Inmuebles", new { id = inmuebleId });
    }
}