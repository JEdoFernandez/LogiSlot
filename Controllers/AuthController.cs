using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using LogiSlot.Data;
using LogiSlot.DTOs;
using LogiSlot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LogiSlot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly LogiSlotDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(LogiSlotDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistroDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuarioExistente = await _context.Usuarios
                .AnyAsync(u => u.Nombre.ToLower() == dto.Nombre.ToLower());

            if (usuarioExistente)
                return BadRequest("El nombre de usuario ya está registrado.");

            var nuevoUsuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                Password = dto.Password, // Almacenado como texto plano según AA1 original
                EsTrabajador = dto.EsTrabajador,
                Activo = true,
                FechaAlta = DateTime.Now
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Usuario registrado correctamente.", UsuarioId = nuevoUsuario.Id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Nombre.ToLower() == dto.Nombre.ToLower() && u.Password == dto.Password && u.Activo);

            if (usuario == null)
                return Unauthorized("Credenciales incorrectas o usuario inactivo.");

            // Generar Token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Clave JWT (mínimo 16 caracteres para HS256, se recomienda de configuración)
            var jwtKey = _configuration["Jwt:Key"] ?? "LogiSlotSuperSecretSecurityKey2026!!!";
            var key = Encoding.UTF8.GetBytes(jwtKey);

            // Determinar rol: true -> Admin, false -> User
            string role = usuario.EsTrabajador ? "Admin" : "User";

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddHours(4),
                Issuer = _configuration["Jwt:Issuer"] ?? "LogiSlotAPI",
                Audience = _configuration["Jwt:Audience"] ?? "LogiSlotClient",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                Token = tokenString,
                Usuario = new
                {
                    usuario.Id,
                    usuario.Nombre,
                    usuario.Email,
                    Rol = role
                }
            });
        }
    }
}
