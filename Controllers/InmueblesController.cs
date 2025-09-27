
using Microsoft.AspNetCore.Mvc;
using Practica2.Data;
using Practica2.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed; 
using System.Text.Json; 
using Practica2.Extensions;

namespace Practica2.Controllers
{
    public class InmueblesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public InmueblesController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Clase auxiliar para guardar la lista y el total en la caché
        private class CacheResult
        {
            public List<Inmueble> Inmuebles { get; set; }
            public int TotalItems { get; set; }
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

            var cacheKey = $"Catalogo_{filtros.Ciudad}_{filtros.Tipo}_{filtros.PrecioMin}_{filtros.PrecioMax}_{filtros.Dormitorios}_Pag{pagina}";
            var cacheBytes = await _cache.GetAsync(cacheKey);
            
            List<Inmueble> listaPaginada;
            int totalItems;
            int pageSize = 10;
            
            if (cacheBytes != null)
            {
                
                var cachedData = JsonSerializer.Deserialize<CacheResult>(cacheBytes);
                listaPaginada = cachedData.Inmuebles;
                totalItems = cachedData.TotalItems;
            }
            else
            {
               
                var inmuebles = _context.Inmuebles.Where(i => i.Activo).AsQueryable();
                
                // Aplicar filtros (Lógica P2)
                if (!string.IsNullOrEmpty(filtros.Ciudad))
                {
                    inmuebles = inmuebles.Where(i => i.Ciudad == filtros.Ciudad);
                }
                if (filtros.Tipo.HasValue) { inmuebles = inmuebles.Where(i => i.Tipo == filtros.Tipo.Value); }
                if (filtros.PrecioMin.HasValue) { inmuebles = inmuebles.Where(i => i.Precio >= filtros.PrecioMin.Value); }
                if (filtros.PrecioMax.HasValue) { inmuebles = inmuebles.Where(i => i.Precio <= filtros.PrecioMax.Value); }
                if (filtros.Dormitorios.HasValue) { inmuebles = inmuebles.Where(i => i.Dormitorios >= filtros.Dormitorios.Value); }
                
                totalItems = await inmuebles.CountAsync();
                
                listaPaginada = await inmuebles
                                        .OrderBy(i => i.Precio)
                                        .Skip((pagina - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync();

                var dataToCache = new CacheResult { Inmuebles = listaPaginada, TotalItems = totalItems };
                var serializedData = JsonSerializer.SerializeToUtf8Bytes(dataToCache);
                
                var options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(60)); // 60 segundos
                    
                await _cache.SetAsync(cacheKey, serializedData, options);
            }

            HttpContext.Session.SetObjectAsJson("UltimosFiltros", filtros); 
            // ----------------------------------------------


            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.CurrentPage = pagina;
            ViewBag.TotalPages = totalPages;
            ViewData["Filtros"] = filtros;

            return View(listaPaginada);
        }
        
        public async Task<IActionResult> Detalle(int id)
        {
            var inmueble = await _context.Inmuebles.FirstOrDefaultAsync(m => m.Id == id);
            if (inmueble == null) return NotFound();

            HttpContext.Session.SetObjectAsJson("UltimoInmueble", new 
            { 
                Id = inmueble.Id, 
                Titulo = inmueble.Titulo 
            });

            var reservaActiva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.InmuebleId == id && r.FechaExpiracion > DateTime.Now);

            ViewBag.ReservaActiva = reservaActiva != null;
            ViewBag.FechaExpiracion = reservaActiva?.FechaExpiracion;

            return View(inmueble);
        }
    }
}