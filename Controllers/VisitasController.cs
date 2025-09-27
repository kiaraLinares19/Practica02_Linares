using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Practica2.Data; // NECESARIO para ApplicationDbContext
using Practica2.Models; // NECESARIO para Visita, Reserva y otros Modelos
using Microsoft.EntityFrameworkCore; // NECESARIO para métodos .FirstOrDefaultAsync()
using System.Security.Claims; // NECESARIO para User.FindFirstValue(ClaimTypes.NameIdentifier)

// Asegura que solo usuarios autenticados puedan acceder a este controlador
[Authorize] 
public class VisitasController : Controller
{
    // 1. CAMPO PRIVADO para la inyección (DEBE ESTAR)
    private readonly ApplicationDbContext _context;

    // 2. CONSTRUCTOR (DEBE ESTAR para inicializar _context)
    public VisitasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ------------------------------------
    // Lógica de AGENDAR VISITA
    // ------------------------------------
    
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

        // Validaciones Server-side (Lógica, Horario Laboral, Solapamiento)
        if (visita.FechaInicio >= visita.FechaFin)
        {
            ModelState.AddModelError(nameof(visita.FechaFin), "La fecha de fin debe ser posterior a la fecha de inicio.");
        }
        if (visita.FechaInicio < DateTime.Now)
        {
            ModelState.AddModelError(nameof(visita.FechaInicio), "No puedes agendar una visita en el pasado.");
        }
        
        [cite_start]// Validar Horario Laboral (Requisito: 08:00 a 19:00) [cite: 48]
        if (visita.FechaInicio.Hour < 8 || visita.FechaFin.Hour > 19)
        {
            ModelState.AddModelError(string.Empty, "Las visitas deben estar dentro del horario laboral (08:00 - 19:00).");
        }
        
        [cite_start]// Validar Solapamiento: Rechazar si ya existe una visita solapada [cite: 23, 46]
        bool isOverlapping = await _context.Visitas
            .AnyAsync(v => 
                v.InmuebleId == visita.InmuebleId &&
                v.Estado != EstadoVisita.Cancelada &&
                (
                    (visita.FechaInicio < v.FechaFin && visita.FechaFin > v.FechaInicio)
                )
            );

        if (isOverlapping)
        {
            ModelState.AddModelError(string.Empty, "¡Error! Ya existe una visita agendada para este inmueble que se solapa con el horario seleccionado.");
        }
        
        if (!ModelState.IsValid)
        {
            // Recargar datos para la vista
            visita.Inmueble = await _context.Inmuebles.FindAsync(visita.InmuebleId);
            return View(visita);
        }

        _context.Add(visita);
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "Visita agendada exitosamente. Espera la confirmación del broker.";
        return RedirectToAction("Detalle", "Inmuebles", new { id = visita.InmuebleId });
    }


    // ------------------------------------
    // Lógica de RESERVA (48 horas)
    // ------------------------------------

    // POST: Procesar la solicitud de reserva
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reservar(int inmuebleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        [cite_start]// 1. Validación de Reserva Activa: Rechazar si ya existe una activa [cite: 24, 47]
        var reservaActiva = await _context.Reservas
            .FirstOrDefaultAsync(r => 
                r.InmuebleId == inmuebleId && 
                r.FechaExpiracion > DateTime.Now);

        if (reservaActiva != null)
        {
            TempData["ErrorMessage"] = "Este inmueble ya tiene una reserva activa.";
            return RedirectToAction("Detalle", "Inmuebles", new { id = inmuebleId });
        }

        // 2. Crear nueva reserva por 48 horas
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