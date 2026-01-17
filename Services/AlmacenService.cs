using LogiSlot.Models;
using System.Text.Json;

namespace LogiSlot.Services;

public class AlmacenService
{
    private const string FilePath = "Data/almacenes.json";
    private List<Almacen> almacenes;

    public AlmacenService()
    {
        almacenes = Cargar();
    }

    private List<Almacen> Cargar()
    {
        if (!File.Exists(FilePath))
            return new List<Almacen>();

        string json = File.ReadAllText(FilePath);

        if (string.IsNullOrWhiteSpace(json))
            return new List<Almacen>();

        return JsonSerializer.Deserialize<List<Almacen>>(json) ?? new List<Almacen>();
    }

    private void Guardar()
    {
        var json = JsonSerializer.Serialize(almacenes, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }

    public void Crear(string nombre, string direccion, int cp)
    {
        var nuevo = new Almacen
        {
            Id = almacenes.Count + 1,
            Nombre = nombre,
            Direccion = direccion,
            CP = cp,
            Activo = true,
            FechaAlta = DateTime.Now
        };

        almacenes.Add(nuevo);
        Guardar();
    }

    public List<Almacen> ObtenerActivos()
    {
        return almacenes.Where(a => a.Activo).ToList();
    }

    public Almacen? ObtenerPorId(int id)
    {
        return almacenes.FirstOrDefault(a => a.Id == id && a.Activo);
    }

    public void Eliminar(int id)
    {
        var a = almacenes.FirstOrDefault(x => x.Id == id);
        if (a != null)
        {
            a.Activo = false;
            Guardar();
        }
    }
    public List<Almacen> BuscarPorCP(int cp)
{
    return almacenes.Where(a => a.Activo && a.CP == cp).ToList();
}
}
