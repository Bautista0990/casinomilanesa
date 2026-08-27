namespace CasinoMilanesaAPI.DTOs;

public record RegistroDto(string Nombre, string Apellido, string Email, string Password,string JuegoFavorito);
public record LoginDto(string Email, string Password);
public record EditarUsuarioDto(string JuegoFavorito);