using System.Linq;
using System.Threading.Tasks;
using LogiSlot.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiSlot.Controllers
{
    // Controlador para administrar a los usuarios del sistema.
    // Solo accesible de forma general por administradores (excepto endpoints marcados con [AllowAnonymous]).
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsuarioController : ControllerBase
    {
        private readonly LogiSlotDbContext _context;

        public UsuarioController(LogiSlotDbContext context)
        {
            _context = context;
        }

        // ZONA PÚBLICA: Obtener todos los usuarios activos sin autenticación
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetUsuariosPublico()
        {
            var usuarios = await _context.Usuarios
                .Where(u => u.Activo)
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Email,
                    u.EsTrabajador,
                    u.FechaAlta
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // Obtener todos los transportistas activos (EsTrabajador = false, Activo = true)
        [HttpGet("transportistas")]
        public async Task<IActionResult> GetTransportistas()
        {
            var transportistas = await _context.Usuarios
                .Where(u => !u.EsTrabajador && u.Activo)
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Email,
                    u.FechaAlta
                })
                .ToListAsync();

            return Ok(transportistas);
        }

        // Eliminar lógicamente (soft-delete) un usuario
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound($"Usuario con ID {id} no encontrado.");

            usuario.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Usuario '{usuario.Nombre}' desactivado correctamente." });
        }
    }
}
