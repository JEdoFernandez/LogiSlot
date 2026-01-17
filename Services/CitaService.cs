using LogiSlot.Models;
using System.Text.Json;

namespace LogiSlot.Services;

public class CitaService
{
    private const string FilePath = "Data/citas.json";
    private List<Cita> citas;

    public CitaService()
    {
        citas = Cargar();
    }

    private List<Cita> Cargar()
    {
        if (!File.Exists(FilePath)) return new();
        var json = File.ReadAllText(FilePath);
        return string.IsNullOrWhiteSpace(json)
            ? new()
            : JsonSerializer.Deserialize<List<Cita>>(json)!;
    }

    private void Guardar()
    {
        File.WriteAllText(FilePath,
            JsonSerializer.Serialize(citas, new JsonSerializerOptions { WriteIndented = true }));
    }

    public bool CrearCita(DateTime fecha, int hora, Almacen almacen, Usuario transportista)
    {
        if (citas.Any(c =>
            c.Activa &&
            c.Fecha.Date == fecha.Date &&
            c.Hora == hora &&
            c.AlmacenId == almacen.Id))
            return false;

        citas.Add(new Cita
        {
            Id = citas.Count + 1,
            Fecha = fecha.Date,
            Hora = hora,
            AlmacenId = almacen.Id,
            AlmacenNombre = almacen.Nombre,
            TransportistaId = transportista.Id,
            TransportistaNombre = transportista.Nombre,
            Activa = true,
            FechaAlta = DateTime.Now
        });

        Guardar();
        return true;
    }

    public List<Cita> ObtenerPorUsuario(int userId)
    {
        return citas.Where(c => c.Activa && c.TransportistaId == userId).ToList();
    }

    public List<Cita> ObtenerTodas()
    {
        return citas.Where(c => c.Activa).ToList();
    }

    public Cita? ObtenerPorId(int id)
    {
        return citas.FirstOrDefault(c => c.Id == id && c.Activa);
    }

    public List<Cita> BuscarPorFecha(DateTime fecha)
    {
        return citas.Where(c => c.Activa && c.Fecha.Date == fecha.Date).ToList();
    }

    // Transportista
    public bool Cancelar(int id, int transportistaId)
    {
        var c = citas.FirstOrDefault(x => x.Id == id && x.TransportistaId == transportistaId && x.Activa);
        if (c == null) return false;

        c.Activa = false;
        Guardar();
        return true;
    }

    // Admin
    public bool Cancelar(int id)
    {
        var c = citas.FirstOrDefault(x => x.Id == id && x.Activa);
        if (c == null) return false;

        c.Activa = false;
        Guardar();
        return true;
    }

    // Slots virtuales
    public List<(int hora, string estado)> ObtenerSlotsVirtuales(DateTime fecha, int almacenId)
    {
        var lista = new List<(int, string)>();

        for (int h = 6; h <= 20; h++)
        {
            var cita = citas.FirstOrDefault(c =>
                c.Activa &&
                c.Fecha.Date == fecha.Date &&
                c.Hora == h &&
                c.AlmacenId == almacenId);

            lista.Add(cita == null
                ? (h, "FREE")
                : (h, cita.TransportistaNombre));
        }

        return lista;
    }
}
