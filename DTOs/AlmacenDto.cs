using System.ComponentModel.DataAnnotations;

namespace LogiSlot.DTOs
{
    public class AlmacenDto
    {
        [Required(ErrorMessage = "El nombre del almacén es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección del almacén es obligatoria.")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código postal es obligatorio.")]
        [Range(1000, 99999, ErrorMessage = "El código postal debe ser un número válido.")]
        public int CP { get; set; }
    }
}
