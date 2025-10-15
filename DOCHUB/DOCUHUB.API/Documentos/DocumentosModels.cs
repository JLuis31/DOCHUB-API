using System.ComponentModel.DataAnnotations;

namespace DOCHUB.APP.Models
{

    public class Documento
    {
        [Key]
        public long idDocumento { get; set; }
        public long usuarioId { get; set; }
        public string titulo { get; set; }
        public string ruta { get; set; }
        public DateTime fechaCarga { get; set; }

    }

    public class EstadoDocumento
    {
        [Key]
        public long idEstado { get; set; }
        public string nombreEstado { get; set; }
        public DateTime fechaActualizacion { get; set; }
        public DateTime fechaCreacion { get; set; }
    }

    public class DocumentoHistorial
    {
        [Key]
        public long iddocumento { get; set; }
        public long idusuario { get; set; }
        public long idestado { get; set; }
        public string? titulo { get; set; }
        public string? ruta { get; set; }
        public DateTime fechacarga { get; set; }

    }



    public class RespuestaDocumentos
    {
        public bool Exito { get; set; }
        public string? Mensaje { get; set; }

        public List<DocumentoHistorial>? Datos { get; set; }
    }



}