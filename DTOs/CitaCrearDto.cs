using System;
using System.ComponentModel.DataAnnotations;

namespace LogiSlot.DTOs
{
    public class CitaCrearDto
    {
        [Required(ErrorMessage = "La fecha de la cita es obligatoria.")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "La hora es obligatoria.")]
        [Range(6, 20, ErrorMessage = "La hora de la cita debe estar entre las 6:00 y las 20:00.")]
        public int Hora { get; set; }

        [Required(ErrorMessage = "El identificador del almacén es obligatorio.")]
        public int AlmacenId { get; set; }
    }
}
