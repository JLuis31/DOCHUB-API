namespace DOCHUB.APP.ModelsDto
{
    public class UsuarioDTO
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }

    public class Respuesta
    {
        public string Mensaje { get; set; }
        public bool Exito { get; set; }
        public int? Id { get; set; }
        public string? Token { get; set; }
    }


}