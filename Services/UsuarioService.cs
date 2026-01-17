using System.Text.Json;
using LogiSlot.Models;

namespace LogiSlot.Services;

public class UsuarioService
{
    private const string FilePath = "Data/usuarios.json";
    private List<Usuario> usuarios;

    public UsuarioService()
    {
        usuarios = Cargar();
    }

    private List<Usuario> Cargar()
    {
        if (!File.Exists(FilePath))
            return new List<Usuario>();

        var json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<List<Usuario>>(json) ?? new List<Usuario>();
    }

    private void Guardar()
    {
        var json = JsonSerializer.Serialize(usuarios, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }

    public Usuario? Registrar(string nombre, string email, string pass, bool esTrabajador)
    {
        if (usuarios.Any(u => u.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            return null;

        var nuevo = new Usuario
        {
            Id = usuarios.Count + 1,
            Nombre = nombre,
            Email = email,
            Password = pass,
            EsTrabajador = esTrabajador
        };

        usuarios.Add(nuevo);
        Guardar();
        return nuevo;
    }

    public Usuario? Login(string nombre, string pass)
    {
        return usuarios.FirstOrDefault(u =>
            u.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)
            && u.Password == pass
            && u.Activo);
    }

    public List<Usuario> ObtenerTransportistas()
        => usuarios.Where(u => !u.EsTrabajador && u.Activo).ToList();

    public void Eliminar(int id)
    {
        var u = usuarios.FirstOrDefault(x => x.Id == id);
        if (u != null)
        {
            u.Activo = false;
            Guardar();
        }
    }

    public List<Usuario> ObtenerTodos() => usuarios;
}
