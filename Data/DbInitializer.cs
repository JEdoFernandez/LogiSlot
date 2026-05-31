using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using LogiSlot.Models;
using Microsoft.EntityFrameworkCore;

namespace LogiSlot.Data
{
    public static class DbInitializer
    {
        public static void Initialize(LogiSlotDbContext context)
        {
            // Aplica las migraciones pendientes y crea la base de datos si no existe
            context.Database.Migrate();

            // Importar Usuarios
            if (!context.Usuarios.Any())
            {
                var path = Path.Combine("Data", "usuarios.json");
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var list = JsonSerializer.Deserialize<List<Usuario>>(json);
                    if (list != null && list.Count > 0)
                    {
                        // Desactivar temporalmente generación automática de identidad si es necesario, 
                        // pero con PostgreSQL EnsureCreated inserta directamente los IDs.
                        context.Usuarios.AddRange(list);
                        context.SaveChanges();

                        // Actualizar secuencia de IDs en PostgreSQL
                        context.Database.ExecuteSqlRaw("SELECT setval(pg_get_serial_sequence('\"Usuarios\"', 'Id'), COALESCE(MAX(\"Id\"), 1)) FROM \"Usuarios\";");
                    }
                }
            }

            // Importar Almacenes
            if (!context.Almacenes.Any())
            {
                var path = Path.Combine("Data", "almacenes.json");
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var list = JsonSerializer.Deserialize<List<Almacen>>(json);
                    if (list != null && list.Count > 0)
                    {
                        context.Almacenes.AddRange(list);
                        context.SaveChanges();

                        // Actualizar secuencia de IDs en PostgreSQL
                        context.Database.ExecuteSqlRaw("SELECT setval(pg_get_serial_sequence('\"Almacenes\"', 'Id'), COALESCE(MAX(\"Id\"), 1)) FROM \"Almacenes\";");
                    }
                }
            }

            // Importar Citas
            if (!context.Citas.Any())
            {
                var path = Path.Combine("Data", "citas.json");
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var list = JsonSerializer.Deserialize<List<Cita>>(json);
                    if (list != null && list.Count > 0)
                    {
                        context.Citas.AddRange(list);
                        context.SaveChanges();

                        // Actualizar secuencia de IDs en PostgreSQL
                        context.Database.ExecuteSqlRaw("SELECT setval(pg_get_serial_sequence('\"Citas\"', 'Id'), COALESCE(MAX(\"Id\"), 1)) FROM \"Citas\";");
                    }
                }
            }
        }
    }
}
