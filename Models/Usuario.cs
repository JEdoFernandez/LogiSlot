namespace LogiSlot.Models;

// Clase que representa a un Usuario del sistema.
// 'EsTrabajador' define si es un administrador del almacén (true) o un transportista (false).
public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public bool EsTrabajador { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; } = DateTime.Now;
}
