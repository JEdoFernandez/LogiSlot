namespace LogiSlot.Models;

// Clase que representa el modelo de una Cita en la base de datos.
// Contiene la información de la reserva: fecha, hora, qué almacén se reserva y quién lo reserva (transportista).
public class Cita
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int Hora { get; set; }
    public int AlmacenId { get; set; }
    public string AlmacenNombre { get; set; } = "";

    public int TransportistaId { get; set; }
    public string TransportistaNombre { get; set; } = "";
    public bool Activa { get; set; }
    public DateTime FechaAlta { get; set; }
}
