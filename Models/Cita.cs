namespace LogiSlot.Models;

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
