using Microsoft.AspNetCore.Mvc;
using Practica2.Data;
using Practica2.Models;
using Microsoft.EntityFrameworkCore;

namespace Practica2.Controllers
{
    public class InmueblesController : Controller
    {

        private readonly ApplicationDbContext _context;


        public InmueblesController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Catalogo([FromQuery] FiltroInmueblesViewModel filtros, int pagina = 1)
        {

            if (!ModelState.IsValid)
            {

                ViewBag.CurrentPage = pagina;
                ViewBag.TotalPages = 1;
                ViewData["Filtros"] = filtros;
                return View(new List<Inmueble>());
            }


            var inmuebles = _context.Inmuebles
                                    .Where(i => i.Activo)
                                    .AsQueryable();

            // Aplicar filtros basados en el ViewModel
            if (!string.IsNullOrEmpty(filtros.Ciudad))
            {
                inmuebles = inmuebles.Where(i => i.Ciudad == filtros.Ciudad);
            }

            if (filtros.Tipo.HasValue)
            {
                inmuebles = inmuebles.Where(i => i.Tipo == filtros.Tipo.Value);
            }

            if (filtros.PrecioMin.HasValue)
            {
                inmuebles = inmuebles.Where(i => i.Precio >= filtros.PrecioMin.Value);
            }

            if (filtros.PrecioMax.HasValue)
            {
                inmuebles = inmuebles.Where(i => i.Precio <= filtros.PrecioMax.Value);
            }

            if (filtros.Dormitorios.HasValue)
            {

                inmuebles = inmuebles.Where(i => i.Dormitorios >= filtros.Dormitorios.Value);
            }


            int pageSize = 10;

            var totalItems = await inmuebles.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);


            var listaPaginada = await inmuebles
                                    .OrderBy(i => i.Precio)
                                    .Skip((pagina - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();


            ViewBag.CurrentPage = pagina;
            ViewBag.TotalPages = totalPages;


            ViewData["Filtros"] = filtros;

            return View(listaPaginada);
        }
        
        public async Task<IActionResult> Detalle(int id)
        {
            var inmueble = await _context.Inmuebles.FirstOrDefaultAsync(m => m.Id == id);
            if (inmueble == null) return NotFound();

            [cite_start]
            var reservaActiva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.InmuebleId == id && r.FechaExpiracion > DateTime.Now);

            ViewBag.ReservaActiva = reservaActiva != null;
            ViewBag.FechaExpiracion = reservaActiva?.FechaExpiracion;

            return View(inmueble);
        }

    }
}