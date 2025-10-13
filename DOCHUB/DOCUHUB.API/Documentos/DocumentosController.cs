using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DOCHUB.APP.ModelsDto;
using DOCHUB.APP.Repositories;
using DOCHUB.Infrastructure.Storage;
using System.Security.Claims;
namespace DOCHUB.APP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class DocumentosController : ControllerBase
    {
        private readonly DocumentosRepository _documentosRepository;
        private readonly CloudflareR2MinioService _r2Service;
        private readonly IConfiguration Configuration;


        public DocumentosController(DocumentosRepository documentosRepository, CloudflareR2MinioService r2Service, IConfiguration configuration2)
        {
            _documentosRepository = documentosRepository;
            _r2Service = r2Service;
            Configuration = configuration2;
        }
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubirDocumentos(
             [FromForm] string idUsuario,
             [FromForm] string nombreArchivo,
             [FromForm] IFormFile archivo)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (archivo == null || archivo.Length == 0)
                return BadRequest(new { mensaje = "No se ha enviado ningún archivo." });

            try
            {
                var url = await _r2Service.SubirArchivoAsync(userId, archivo, archivo.FileName);

                var respuesta = await _documentosRepository.SubirDocumentos(userId, nombreArchivo, url);

                if (respuesta.Exito)
                    return Ok(new
                    {
                        mensaje = "Archivo subido y registrado correctamente.",
                    });

                return BadRequest(respuesta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al subir el documento", detalle = ex.Message });
            }
        }

        [HttpGet]
        [Route("documentos-usuario")]
        public async Task<IActionResult> ObtenerDocumentos(int idUsuario)
        {
            var userId = User.FindFirst("UserId")?.Value;

            var respuesta = await _documentosRepository.ObtenerDocumentos(userId);
            if (respuesta.Exito)
            {
                return Ok(respuesta);
            }
            return BadRequest(respuesta);
        }

        [HttpGet]
        [Route("documentos-activos")]
        public async Task<IActionResult> ObtenerDocumentosActivos(int idUsuario)
        {
            var userId = User.FindFirst("UserId")?.Value;

            var respuesta = await _documentosRepository.ObtenerDocumentosActivos(userId);
            if (respuesta.Exito)
            {
                return Ok(respuesta);
            }
            return BadRequest(respuesta);
        }

        [HttpDelete]
        [Route("eliminarDocumento")]
        public async Task<IActionResult> EliminarDocumento(string idUsuario, string titulo, string fechaCarga)
        {
            var userId = User.FindFirst("UserId")?.Value;

            Console.WriteLine($"ID Usuario: {userId}, Título: {titulo}, Fecha Carga: {fechaCarga}");
            if (userId == null)
            {
                return BadRequest(new Respuesta { Exito = false, Mensaje = "ID de usuario es requerido." });
            }
            await _r2Service.EliminarArchivoAsync(userId, titulo);
            var respuesta = await _documentosRepository.EliminarDocumento(userId, titulo, fechaCarga);
            if (respuesta.Exito)
            {
                return Ok(respuesta);
            }
            return BadRequest(respuesta);
        }

        [HttpGet]
        [Route("archivosRecientes")]
        public async Task<IActionResult> ObtenerArchivosRecientes(string idUsuario, string titulo, string ruta)
        {
            var userId = User.FindFirst("UserId")?.Value;

            if (userId == null)
            {
                return BadRequest(new Respuesta { Exito = false, Mensaje = "ID de usuario es requerido." });
            }

            var respuesta = await _r2Service.ObtenerArchivoAsync(userId, titulo, ruta);
            if (respuesta is OkObjectResult)
            {
                return Ok(respuesta);
            }
            return BadRequest(respuesta);
        }

        [HttpGet("descargarDocumento")]
        public async Task<IActionResult> DescargarArchivo(string titulo, string ruta)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null)
                return BadRequest(new { exito = false, mensaje = "ID de usuario es requerido." });

            try
            {
                var memoryStream = await _r2Service.DescargarArchivoAsync(userId, titulo);

                var contentType = titulo.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                    ? "application/pdf"
                    : titulo.EndsWith(".docx", StringComparison.OrdinalIgnoreCase)
                        ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                    : titulo.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
                        ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    : "application/octet-stream";

                return File(memoryStream.ToArray(), contentType, titulo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { exito = false, mensaje = $"Error al descargar desde R2: {ex.Message}" });
            }
        }
        [HttpGet("generar-url-firmada")]
        public async Task<IActionResult> GenerarUrlFirmada(string titulo)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null)
                return BadRequest(new { exito = false, mensaje = "Debe iniciar sesión para acceder al archivo." });

            try
            {
                var url = await _r2Service.GenerarUrlFirmadaAsync(userId, titulo, 120);
                return Ok(new { exito = true, url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { exito = false, mensaje = ex.Message });
            }
        }


    }

}
