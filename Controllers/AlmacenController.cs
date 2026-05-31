using System;
using System.Linq;
using System.Threading.Tasks;
using LogiSlot.Data;
using LogiSlot.DTOs;
using LogiSlot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiSlot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlmacenController : ControllerBase
    {
        private readonly LogiSlotDbContext _context;

        public AlmacenController(LogiSlotDbContext context)
        {
            _context = context;
        }

        // ZONA PÚBLICA: Obtener almacenes activos con filtrado y ordenación
        // Filtrado por 'nombre' y 'cp'. Ordenación por 'nombre', 'cp' o 'fechaAlta'
        [HttpGet]
        public async Task<IActionResult> GetAlmacenes(
            [FromQuery] string? nombre,
            [FromQuery] int? cp,
            [FromQuery] string sortBy = "nombre",
            [FromQuery] bool isDescending = false)
        {
            var query = _context.Almacenes.Where(a => a.Activo);

            // Filtrado
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                query = query.Where(a => a.Nombre.ToLower().Contains(nombre.ToLower()));
            }

            if (cp.HasValue)
            {
                query = query.Where(a => a.CP == cp.Value);
            }

            // Ordenación
            sortBy = sortBy.ToLower();
            if (sortBy == "cp")
            {
                query = isDescending ? query.OrderByDescending(a => a.CP) : query.OrderBy(a => a.CP);
            }
            else if (sortBy == "fechaalta")
            {
                query = isDescending ? query.OrderByDescending(a => a.FechaAlta) : query.OrderBy(a => a.FechaAlta);
            }
            else // Por defecto ordenado por Nombre
            {
                query = isDescending ? query.OrderByDescending(a => a.Nombre) : query.OrderBy(a => a.Nombre);
            }

            var almacenes = await query.ToListAsync();
            return Ok(almacenes);
        }

        // ZONA PÚBLICA: Obtener almacén por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAlmacenById(int id)
        {
            var almacen = await _context.Almacenes.FirstOrDefaultAsync(a => a.Id == id && a.Activo);
            if (almacen == null)
                return NotFound($"Almacén con ID {id} no encontrado o inactivo.");

            return Ok(almacen);
        }

        // ZONA PRIVADA (Admin): Crear almacén
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAlmacen([FromBody] AlmacenDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var nuevoAlmacen = new Almacen
            {
                Nombre = dto.Nombre,
                Direccion = dto.Direccion,
                CP = dto.CP,
                Activo = true,
                FechaAlta = DateTime.Now
            };

            _context.Almacenes.Add(nuevoAlmacen);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAlmacenById), new { id = nuevoAlmacen.Id }, nuevoAlmacen);
        }

        // ZONA PRIVADA (Admin): Eliminar/Desactivar almacén (soft-delete)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAlmacen(int id)
        {
            var almacen = await _context.Almacenes.FindAsync(id);
            if (almacen == null)
                return NotFound($"Almacén con ID {id} no encontrado.");

            almacen.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Almacén '{almacen.Nombre}' desactivado correctamente." });
        }
    }
}
