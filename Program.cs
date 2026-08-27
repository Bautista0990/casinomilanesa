using Microsoft.EntityFrameworkCore;
using CasinoMilanesaAPI.Data;
using CasinoMilanesaAPI.Models;
using CasinoMilanesaAPI.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Validación para detectar rápidamente si la variable no llegó
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("La variable 'ConnectionStrings__DefaultConnection' no está configurada o llegó vacía.");
}

var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion, mySqlOptions =>
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var app = builder.Build();
// Aplica las migraciones automáticamente al arrancar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
app.UseCors();

app.MapGet("/", () => "🎰 API Casino La Milanesa Giratoria está online 🥩");

// Auth Endpoints
app.MapPost("/api/auth/registro", async (AppDbContext db, RegistroDto dto) =>
{
    if (await db.Usuarios.AnyAsync(u => u.Email == dto.Email))
        return Results.BadRequest(new { mensaje = "El correo ya está registrado." });

    var usuario = new Usuario
    {
        Nombre = dto.Nombre,
        Apellido = dto.Apellido,
        Email = dto.Email,
        PasswordHash = dto.Password + "_hashTrucho"
    };

    db.Usuarios.Add(usuario);
    await db.SaveChangesAsync();

    return Results.Ok(new { mensaje = "Usuario registrado exitosamente." });
});

app.MapPost("/api/auth/login", async (AppDbContext db, LoginDto dto) =>
{
    var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);
    
    if (usuario == null)
        return Results.Unauthorized(); // <-- Devuelve 401 si no encuentra el mail

    // Verificar contraseña
    if (usuario.Password != dto.Password) 
        return Results.Unauthorized(); // <-- Devuelve 401 si la clave no coincide

    return Results.Ok(new { usuario.Id, usuario.Nombre, usuario.Email });
});

// Admin Endpoints
app.MapGet("/api/admin/usuarios", async (AppDbContext db, string? filtro) =>
{
    var query = db.Usuarios.AsQueryable();

    if (!string.IsNullOrWhiteSpace(filtro))
    {
        query = query.Where(u => u.Nombre.Contains(filtro) || u.Apellido.Contains(filtro));
    }

    var usuarios = await query
        .Select(u => new { u.Id, u.Nombre, u.Apellido, u.Email, u.JuegoFavorito, u.Estado, u.Rol })
        .ToListAsync();

    return Results.Ok(usuarios);
});

app.MapPut("/api/admin/usuarios/{id:int}", async (AppDbContext db, int id, EditarUsuarioDto dto) =>
{
    var user = await db.Usuarios.FindAsync(id);
    if (user == null) return Results.NotFound();

    user.JuegoFavorito = dto.JuegoFavorito;
    await db.SaveChangesAsync();

    return Results.Ok(new { mensaje = "Datos actualizados correctamente." });
});

app.MapPut("/api/admin/usuarios/{id:int}/ban", async (AppDbContext db, int id) =>
{
    var user = await db.Usuarios.FindAsync(id);
    if (user == null) return Results.NotFound();

    user.Estado = "baneado";
    await db.SaveChangesAsync();

    return Results.Ok(new { mensaje = "Usuario dado de baja exitosamente." });
});
var builder = WebApplication.CreateBuilder(args);

// 1. Agregar política CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ... resto de tus servicios

var app = builder.Build();

// 2. Habilitar CORS en el pipeline (IMPORTANTE: colocar antes de MapControllers)
app.UseCors("AllowAll");

app.MapControllers();
app.Run();