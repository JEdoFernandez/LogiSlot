using LogiSlot.Models;
using LogiSlot.Services;
using System.Globalization;

Directory.CreateDirectory("Data");

var usuarioService = new UsuarioService();
var almacenService = new AlmacenService();
var citaService = new CitaService();

Usuario? usuarioActual = null;
bool salir = false;
CultureInfo culture = new("es-ES");

//  MENU PRINCIPAL 

while (!salir)
{
    Console.Clear();
    Console.WriteLine("=== LOGISLOT ===\n");

    if (usuarioActual == null)
    {
        Console.WriteLine("1. Iniciar sesión");
        Console.WriteLine("2. Registrar usuario");
        Console.WriteLine("3. Ver slots por día");
        Console.WriteLine("4. Salir");
        Console.Write("\nOpción: ");

        switch (Console.ReadLine())
        {
            case "1": Login(); break;
            case "2": Registro(); break;
            case "3": VistaPublicaSlots(); break;
            case "4": salir = true; break;
        }
    }
    else
    {
        if (usuarioActual.EsTrabajador)
            MenuAdmin();
        else
            MenuTransportista();
    }
}

//  MENU TRANSPORTISTA

void MenuTransportista()
{
    Console.Clear();
    Console.WriteLine($"=== TRANSPORTISTA: {usuarioActual!.Nombre} ===\n");

    Console.WriteLine("1. Ver mis citas");
    Console.WriteLine("2. Crear cita");
    Console.WriteLine("3. Cancelar cita");
    Console.WriteLine("4. Buscar mis citas");
    Console.WriteLine("5. Buscar almacén por CP");
    Console.WriteLine("6. Cerrar sesión");

    switch (Console.ReadLine())
    {
        case "1": VerMisCitas(); break;
        case "2": CrearCita(); break;
        case "3": CancelarCitaPropia(); break;
        case "4": BuscarMisCitas(); break;
        case "5": BuscarAlmacen(); break;
        case "6": usuarioActual = null; break;
    }
}

//  MENU TRABAJADOR (ADMIN)
void MenuAdmin()
{
    Console.Clear();
    Console.WriteLine($"=== ADMIN: {usuarioActual!.Nombre} ===\n");

    Console.WriteLine("1. Ver todas las citas");
    Console.WriteLine("2. Cancelar cita");
    Console.WriteLine("3. Crear almacén");
    Console.WriteLine("4. Ver transportistas");
    Console.WriteLine("5. Ver almacenes");
    Console.WriteLine("6. Buscar citas");
    Console.WriteLine("7. Cerrar sesión");

    switch (Console.ReadLine())
    {
        case "1": VerTodasLasCitas(); break;
        case "2": CancelarCitaAdmin(); break;
        case "3": CrearAlmacen(); break;
        case "4": VerTransportistas(); break;
        case "5": VerAlmacenesAdmin(); break;
        case "6": BuscarCitasAdmin(); break;
        case "7": usuarioActual = null; break;
    }
}

// BACKEND MENU PRINCIPAL 

void Login()
{
    Console.Clear();
    Console.Write("Nombre: ");
    string nombre = Console.ReadLine()!;
    Console.Write("Contraseña: ");
    string pass = Console.ReadLine()!;

    usuarioActual = usuarioService.Login(nombre, pass);
    Console.WriteLine(usuarioActual == null ? "Credenciales incorrectas" : "Login correcto");
    Console.ReadKey();
}

void Registro()
{
    Console.Clear();
    Console.Write("Nombre: ");
    string nombre = Console.ReadLine()!;
    Console.Write("Email: ");
    string email = Console.ReadLine()!;
    Console.Write("Contraseña: ");
    string pass = Console.ReadLine()!;
    Console.Write("¿Es trabajador? (s/n): ");
    bool esTrabajador = Console.ReadLine()!.ToLower() == "s";

    var nuevo = usuarioService.Registrar(nombre, email, pass, esTrabajador);
    Console.WriteLine(nuevo == null ? "Nombre ya existente." : "Usuario creado.");
    Console.ReadKey();
}

void VistaPublicaSlots()
{
    var fecha = PedirFecha();
    var almacen = SeleccionarAlmacen();
    if (almacen == null) return;

    MostrarSlots(fecha, almacen.Id);
    Console.ReadKey();
}

// BACKEND TRANSPORTISTA 

void VerMisCitas()
{
    Console.Clear();
    var citas = citaService.ObtenerPorUsuario(usuarioActual!.Id);

    if (!citas.Any())
        Console.WriteLine("No tienes citas.");
    else
        citas.ForEach(c =>
            Console.WriteLine($"{c.Id} | {c.Fecha:dd-MM-yyyy} {c.Hora}:00 | {c.AlmacenNombre}")
        );

    Console.ReadKey();
}

void CrearCita()
{
    var fecha = PedirFecha();
    var almacen = SeleccionarAlmacen();
    if (almacen == null) return;

    MostrarSlots(fecha, almacen.Id);

    int hora;
    do
    {
        Console.Write("\nHora (6 - 20): ");
    } while (!int.TryParse(Console.ReadLine(), out hora) || hora < 6 || hora > 20);

    bool ok = citaService.CrearCita(fecha, hora, almacen, usuarioActual!);
    Console.WriteLine(ok ? "Cita creada." : "Ese slot ya está ocupado.");
    Console.ReadKey();
}

void CancelarCitaPropia()
{
    Console.Clear();
    VerMisCitas();

    Console.Write("\nID a cancelar: ");
    if (int.TryParse(Console.ReadLine(), out int id))
    {
        var ok = citaService.Cancelar(id, usuarioActual!.Id);
        Console.WriteLine(ok ? "Cita cancelada." : "No es tu cita o no existe.");
    }
    Console.ReadKey();
}

void BuscarMisCitas()
{
    Console.Clear();
    Console.Write("Fecha (dd-MM-yyyy): ");
    if (DateTime.TryParseExact(Console.ReadLine(), "dd-MM-yyyy", culture, DateTimeStyles.None, out DateTime fecha))
    {
        var lista = citaService.ObtenerPorUsuario(usuarioActual!.Id)
                               .Where(c => c.Fecha.Date == fecha.Date).ToList();

        if (!lista.Any()) Console.WriteLine("No hay citas.");
        else lista.ForEach(c => Console.WriteLine($"{c.Id} | {c.Fecha:dd-MM-yyyy} {c.Hora}:00 | {c.AlmacenNombre}"));
    }
    Console.ReadKey();
}

// BACKEND TRABAJADOR (ADMIN)
void VerTodasLasCitas()
{
    Console.Clear();
    citaService.ObtenerTodas().ForEach(c =>
        Console.WriteLine($"{c.Id} | {c.Fecha:dd-MM-yyyy} {c.Hora}:00 | {c.AlmacenNombre} | {c.TransportistaNombre}")
    );
    Console.ReadKey();
}

void CancelarCitaAdmin()
{
    Console.Clear();
    VerTodasLasCitas();
    Console.Write("\nID a cancelar: ");
    if (int.TryParse(Console.ReadLine(), out int id))
        citaService.Cancelar(id);
    Console.ReadKey();
}

void CrearAlmacen()
{
    Console.Clear();
    Console.Write("Nombre: ");
    string n = Console.ReadLine()!;
    Console.Write("Dirección: ");
    string d = Console.ReadLine()!;
    Console.Write("CP: ");
    int cp = int.Parse(Console.ReadLine()!);

    almacenService.Crear(n, d, cp);
}

void VerTransportistas()
{
    Console.Clear();
    var lista = usuarioService.ObtenerTransportistas();
    lista.ForEach(u => Console.WriteLine($"{u.Id}. {u.Nombre}"));

    Console.Write("\nID a eliminar o ENTER: ");
    if (int.TryParse(Console.ReadLine(), out int id))
        usuarioService.Eliminar(id);

    Console.ReadKey();
}

void VerAlmacenesAdmin()
{
    Console.Clear();
    var lista = almacenService.ObtenerActivos();
    lista.ForEach(a => Console.WriteLine($"{a.Id}. {a.Nombre} - {a.Direccion}"));

    Console.Write("\nID a eliminar o ENTER: ");
    if (int.TryParse(Console.ReadLine(), out int id))
        almacenService.Eliminar(id);

    Console.ReadKey();
}

void BuscarCitasAdmin()
{
    Console.Clear();
    Console.Write("ID o fecha (dd-MM-yyyy): ");
    string input = Console.ReadLine()!;

    if (int.TryParse(input, out int id))
    {
        var c = citaService.ObtenerPorId(id);
        if (c != null)
            Console.WriteLine($"{c.Id} | {c.Fecha:dd-MM-yyyy} {c.Hora}:00 | {c.AlmacenNombre}");
    }
    else if (DateTime.TryParseExact(input, "dd-MM-yyyy", culture, DateTimeStyles.None, out DateTime fecha))
    {
        citaService.BuscarPorFecha(fecha).ForEach(c =>
            Console.WriteLine($"{c.Id} | {c.Fecha:dd-MM-yyyy} {c.Hora}:00 | {c.AlmacenNombre}")
        );
    }

    Console.ReadKey();
}

// FUNCIONES BACKEND GENERICAS

void BuscarAlmacen()
{
    Console.Clear();
    Console.Write("Introduce CP: ");
    int cp = int.Parse(Console.ReadLine()!);

    var lista = almacenService.BuscarPorCP(cp);

    if (!lista.Any())
        Console.WriteLine("No hay almacenes con ese CP.");
    else
        lista.ForEach(a =>
            Console.WriteLine($"{a.Id} | {a.Nombre} | {a.Direccion} | {a.CP}")
        );

    Console.ReadKey();
}

DateTime PedirFecha()
{
    Console.Write("\nFecha (dd-MM-yyyy): ");
    return DateTime.ParseExact(Console.ReadLine()!, "dd-MM-yyyy", culture);
}

Almacen? SeleccionarAlmacen()
{
    var almacenes = almacenService.ObtenerActivos();
    if (!almacenes.Any())
    {
        Console.WriteLine("No hay almacenes creados.");
        Console.ReadKey();
        return null;
    }

    Console.WriteLine("\nAlmacenes:");
    almacenes.ForEach(a => Console.WriteLine($"{a.Id}. {a.Nombre}"));

    Console.Write("Selecciona ID: ");
    int id = int.Parse(Console.ReadLine()!);
    return almacenService.ObtenerPorId(id);
}

void MostrarSlots(DateTime fecha, int almacenId)
{
    Console.Clear();
    var slots = citaService.ObtenerSlotsVirtuales(fecha, almacenId);

    Console.WriteLine($"SLOTS {fecha:dd-MM-yyyy}\n");

    foreach (var s in slots)
        Console.WriteLine($"{s.hora}:00 - {s.estado}");
}
