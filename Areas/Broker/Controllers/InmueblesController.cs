using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Practica2.Data;
using Practica2.Models;

[Area("Broker")] 
[Authorize(Roles = "Broker")] 
public class InmueblesController : Controller
{
    private readonly ApplicationDbContext _context;

    public InmueblesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var inmuebles = await _context.Inmuebles.ToListAsync();
        return View(inmuebles);
    }
    
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Inmueble inmueble)
    {
        if (ModelState.IsValid)
        {
            _context.Add(inmueble);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Inmueble creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        return View(inmueble);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var inmueble = await _context.Inmuebles.FindAsync(id);
        if (inmueble == null) return NotFound();

        return View(inmueble);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Inmueble inmueble)
    {
        if (id != inmueble.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(inmueble);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Inmueble actualizado exitosamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Inmuebles.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(inmueble);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActivo(int id)
    {
        var inmueble = await _context.Inmuebles.FindAsync(id);
        if (inmueble == null) return NotFound();

        inmueble.Activo = !inmueble.Activo;
        _context.Update(inmueble);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = inmueble.Activo ? "Inmueble activado." : "Inmueble desactivado.";
        return RedirectToAction(nameof(Index));
    }
}