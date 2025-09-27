
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Practica2.Data;
using Practica2.Models;

[Area("Broker")]
[Authorize(Roles = "Broker")]
public class AgendaController : Controller
{
    private readonly ApplicationDbContext _context;

    public AgendaController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var hoy = DateTime.Today;
        var visitas = await _context.Visitas
            .Include(v => v.Inmueble)
            .Include(v => v.Usuario) 
            .Where(v => v.FechaInicio.Date >= hoy && v.Estado != EstadoVisita.Cancelada)
            .OrderBy(v => v.FechaInicio)
            .ToListAsync();

        return View(visitas);
    }

    [HttpPost]
    public async Task<IActionResult> ChangeVisitaEstado(int id, EstadoVisita nuevoEstado)
    {
        var visita = await _context.Visitas.FindAsync(id);
        if (visita == null) return NotFound();

        if (nuevoEstado == EstadoVisita.Confirmada || nuevoEstado == EstadoVisita.Cancelada)
        {
            visita.Estado = nuevoEstado;
            _context.Update(visita);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Visita {visita.Inmueble.Titulo} fue {nuevoEstado.ToString()}.";
        }
        else
        {
             TempData["ErrorMessage"] = "Estado no válido para la acción.";
        }

        return RedirectToAction(nameof(Index));
    }
    
    public async Task<IActionResult> Reservas()
    {
        var reservas = await _context.Reservas
            .Include(r => r.Inmueble)
            .Include(r => r.Usuario)
            .Where(r => r.FechaExpiracion > DateTime.Now)
            .OrderBy(r => r.FechaExpiracion)
            .ToListAsync();

        return View(reservas);
    }

    [HttpPost]
    public async Task<IActionResult> LiberarReserva(int id)
    {
        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva == null) return NotFound();

        reserva.FechaExpiracion = DateTime.Now.AddSeconds(-1); 
        
        _context.Update(reserva);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Reserva del inmueble {reserva.Inmueble.Titulo} liberada exitosamente.";
        return RedirectToAction(nameof(Reservas));
    }
}