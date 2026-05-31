using System.ComponentModel.DataAnnotations;

namespace LogiSlot.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = string.Empty;
    }
}
