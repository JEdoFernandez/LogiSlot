using System.Text;
using LogiSlot.Data;
using LogiSlot.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Habilitar comportamiento heredado de timestamps en Npgsql para permitir DateTime.Now (Local) en PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar DbContext de EF Core para PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=logislot_db;Database=logislot;Username=postgres;Password=postgrespassword";
builder.Services.AddDbContext<LogiSlotDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Configurar Autenticación JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "LogiSlotSuperSecretSecurityKey2026!!!";
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "LogiSlotAPI",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "LogiSlotClient",
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

// 3. Configurar Controladores y JSON
builder.Services.AddControllers();

// 4. Configurar Swagger/OpenAPI con soporte para Bearer Token
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "LogiSlot RESTful API", 
        Version = "v1",
        Description = "API RESTful para la gestión de slots y citas en almacenes LogiSlot (AA2)"
    });

    // Configurar seguridad JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Introduce el token JWT usando el esquema Bearer. Ejemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Registrar el filtro para aplicar la seguridad únicamente en endpoints con el atributo [Authorize]
    c.OperationFilter<AuthorizeCheckOperationFilter>();
});

var app = builder.Build();

// 5. Inicializar/Sembrar la Base de Datos automáticamente desde los archivos JSON
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<LogiSlotDbContext>();
        DbInitializer.Initialize(context);
        Console.WriteLine("--> Base de datos inicializada y sembrada con éxito.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> Error durante la inicialización de la base de datos: {ex.Message}");
    }
}

// 6. Configurar Middleware HTTP
if (app.Environment.IsDevelopment() || true) // Permitir Swagger en cualquier entorno para facilitar la entrega
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LogiSlot API V1");
        c.RoutePrefix = "swagger"; // Swagger estará disponible en http://localhost:PORT/swagger
    });
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
