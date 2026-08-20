using Microsoft.EntityFrameworkCore;
using CasinoMilanesaAPI.Models;

namespace CasinoMilanesaAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Apellido).HasColumnName("apellido").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Rol).HasColumnName("rol").HasDefaultValue("jugador");
            entity.Property(e => e.JuegoFavorito).HasColumnName("juego_favorito").HasMaxLength(50).HasDefaultValue("Tragamonedas");
            entity.Property(e => e.Estado).HasColumnName("estado").HasDefaultValue("activo");
            entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<TuEntidad>() // Reemplazá por el nombre de la clase (ej: Usuario)
    .Property(e => e.FechaRegistro)
    .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}