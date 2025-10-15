using System.ComponentModel.DataAnnotations;

namespace DOCHUB.APP.ModelsDto
{
    public class UsuarioDTO
    {
        public string Nombre { get; set; }
        public string email { get; set; }
        public string Password { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }

    public class Respuesta
    {
        public string Mensaje { get; set; }
        public bool Exito { get; set; }
        public long? Id { get; set; }
        public string? Token { get; set; }
    }

    public class DocumentoHistorialDto
    {
        [Key]
        public long iddocumento { get; set; }
        public long idusuario { get; set; }
        public string idestado { get; set; }
        public string? titulo { get; set; }
        public string? ruta { get; set; }
        public DateTime fechacarga { get; set; }

    }

    public class RespuestaDocumentosDto
    {
        public bool Exito { get; set; }
        public string? Mensaje { get; set; }

        public List<DocumentoHistorialDto>? Datos { get; set; }
    }


}