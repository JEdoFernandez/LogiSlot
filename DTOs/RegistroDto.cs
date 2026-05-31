using System.ComponentModel.DataAnnotations;

namespace LogiSlot.DTOs
{
    public class RegistroDto
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(4, ErrorMessage = "La contraseña debe tener al menos 4 caracteres.")]
        public string Password { get; set; } = string.Empty;

        public bool EsTrabajador { get; set; }
    }
}
