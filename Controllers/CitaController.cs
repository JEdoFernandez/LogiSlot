using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    public class CitaController : ControllerBase
    {
        private readonly LogiSlotDbContext _context;

        public CitaController(LogiSlotDbContext context)
        {
            _context = context;
        }

        // ZONA PRIVADA (Admin): Obtener todas las citas activas
        [HttpGet("todas")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCitas()
        {
            var citas = await _context.Citas
                .Where(c => c.Activa)
                .OrderBy(c => c.Fecha)
                .ThenBy(c => c.Hora)
                .ToListAsync();

            return Ok(citas);
        }

        // ZONA PRIVADA (Transportista/User/Admin): Obtener mis citas con filtrado y ordenación
        // Filtrado por 'fecha' (solo fecha, ignorando hora) y 'hora'.
        // Ordenación por 'fecha' u 'hora'.
        [HttpGet("mis-citas")]
        [Authorize]
        public async Task<IActionResult> GetMisCitas(
            [FromQuery] DateTime? fecha,
            [FromQuery] int? hora,
            [FromQuery] string sortBy = "fecha",
            [FromQuery] bool isDescending = false)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized("Token no válido o identificador de usuario ausente.");

            // Si es Admin, puede ver las citas de todos, pero por defecto mostramos las suyas
            // a menos que se filtre. Para cumplir con la zona privada del transportista,
            // filtramos por su TransportistaId.
            var query = _context.Citas.Where(c => c.Activa && c.TransportistaId == userId);

            // Filtrado
            if (fecha.HasValue)
            {
                var targetDate = fecha.Value.Date;
                query = query.Where(c => c.Fecha.Date == targetDate);
            }

            if (hora.HasValue)
            {
                query = query.Where(c => c.Hora == hora.Value);
            }

            // Ordenación
            sortBy = sortBy.ToLower();
            if (sortBy == "hora")
            {
                query = isDescending ? query.OrderByDescending(c => c.Hora) : query.OrderBy(c => c.Hora);
            }
            else // Por defecto ordenado por fecha
            {
                query = isDescending ? query.OrderByDescending(c => c.Fecha) : query.OrderBy(c => c.Fecha);
            }

            var citas = await query.ToListAsync();
            return Ok(citas);
        }

        // ZONA PÚBLICA: Ver slots virtuales por día y almacén
        [HttpGet("publica/slots")]
        public async Task<IActionResult> GetSlotsPublicos(
            [FromQuery] DateTime fecha,
            [FromQuery] int almacenId)
        {
            var almacen = await _context.Almacenes.FirstOrDefaultAsync(a => a.Id == almacenId && a.Activo);
            if (almacen == null)
                return NotFound($"El almacén con ID {almacenId} no existe o está inactivo.");

            var targetDate = fecha.Date;

            // Obtener citas activas en esa fecha y almacén
            var citas = await _context.Citas
                .Where(c => c.Activa && c.Fecha.Date == targetDate && c.AlmacenId == almacenId)
                .ToListAsync();

            var slots = new List<object>();

            for (int h = 6; h <= 20; h++)
            {
                var cita = citas.FirstOrDefault(c => c.Hora == h);
                slots.Add(new
                {
                    Hora = $"{h:D2}:00",
                    Estado = cita == null ? "FREE" : cita.TransportistaNombre
                });
            }

            return Ok(new
            {
                Fecha = targetDate.ToString("yyyy-MM-dd"),
                Almacen = almacen.Nombre,
                Slots = slots
            });
        }

        // ZONA PRIVADA (User/Admin): Crear una cita
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCita([FromBody] CitaCrearDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userNameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized("Token no válido o identificador de usuario ausente.");

            // Validar que el almacén existe y está activo
            var almacen = await _context.Almacenes.FirstOrDefaultAsync(a => a.Id == dto.AlmacenId && a.Activo);
            if (almacen == null)
                return BadRequest("El almacén especificado no existe o está inactivo.");

            // Validar que el transportista existe y está activo
            var transportista = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId && u.Activo);
            if (transportista == null)
                return BadRequest("El transportista no existe o está inactivo.");

            var targetDate = dto.Fecha.Date;

            // Validar si el slot ya está ocupado
            var slotOcupado = await _context.Citas
                .AnyAsync(c => c.Activa && c.Fecha.Date == targetDate && c.Hora == dto.Hora && c.AlmacenId == dto.AlmacenId);

            if (slotOcupado)
                return BadRequest("El slot seleccionado para esta fecha y almacén ya está ocupado.");

            // Crear la cita
            var nuevaCita = new Cita
            {
                Fecha = targetDate,
                Hora = dto.Hora,
                AlmacenId = almacen.Id,
                AlmacenNombre = almacen.Nombre,
                TransportistaId = transportista.Id,
                TransportistaNombre = transportista.Nombre,
                Activa = true,
                FechaAlta = DateTime.Now
            };

            _context.Citas.Add(nuevaCita);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSlotsPublicos), new { fecha = targetDate, almacenId = almacen.Id }, nuevaCita);
        }

        // ZONA PRIVADA (User/Admin): Cancelar una cita (soft-delete)
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> CancelCita(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized("Token no válido o identificador de usuario ausente.");

            var cita = await _context.Citas.FindAsync(id);
            if (cita == null || !cita.Activa)
                return NotFound($"La cita activa con ID {id} no existe.");

            // Restricción de propiedad: Si no es Admin, solo puede cancelar sus propias citas
            if (userRoleClaim != "Admin" && cita.TransportistaId != userId)
                return Forbid("No tienes autorización para cancelar la cita de otro transportista.");

            cita.Activa = false;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Cita con ID {id} cancelada correctamente." });
        }
    }
}
