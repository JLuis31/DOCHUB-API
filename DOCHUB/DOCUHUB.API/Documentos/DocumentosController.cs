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


        public DocumentosController(DocumentosRepository documentosRepository, CloudflareR2MinioService r2Service)
        {
            _documentosRepository = documentosRepository;
            _r2Service = r2Service;
        }
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubirDocumentos(
             [FromForm] string idUsuario,
             [FromForm] string nombreArchivo,
             [FromForm] IFormFile archivo
             , [FromForm] int tamañoArchivo)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (archivo == null || archivo.Length == 0)
                return BadRequest(new { mensaje = "No se ha enviado ningún archivo." });
            try
            {
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
                string nombreSinExtension = Path.GetFileNameWithoutExtension(archivo.FileName);
                string extension = Path.GetExtension(archivo.FileName);

                string titulo = $"{nombreSinExtension}_{timestamp}{extension}";
                var result = await _r2Service.ObtenerCantidadMbUsuario(idUsuario);
                var okResult = result as OkObjectResult;
                if (okResult != null)
                {
                    dynamic? value = okResult.Value;
                    double? mb = value.megabytesUsados;
                    var resta = 100 - mb;
                    if (resta < tamañoArchivo)
                    {
                        return BadRequest(new { mensaje = "No tienes suficiente espacio para subir este archivo." });
                    }
                    if (mb >= 100)
                    {
                        return BadRequest(new { mensaje = "Has alcanzado el límite de almacenamiento" });
                    }
                }

                var url = await _r2Service.SubirArchivoAsync(userId, archivo, titulo);

                var respuesta = await _documentosRepository.SubirDocumentos(userId, titulo, url);

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
        public async Task<IActionResult> ObtenerDocumentos(long idUsuario)
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
        public async Task<IActionResult> ObtenerDocumentosActivos()
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
        public async Task<IActionResult> EliminarDocumento(string idUsuario, string titulo)
        {
            var userId = User.FindFirst("UserId")?.Value;

            if (userId == null)
            {
                return BadRequest(new Respuesta { Exito = false, Mensaje = "ID de usuario es requerido." });
            }
            await _r2Service.EliminarArchivoAsync(userId, titulo);
            var respuesta = await _documentosRepository.EliminarDocumento(userId, titulo);
            if (respuesta.Exito)
            {
                return Ok(respuesta);
            }
            return BadRequest(respuesta);
        }

        [HttpGet]
        [Route("archivosRecientes")]
        public async Task<IActionResult> ObtenerArchivosRecientes(string titulo, string ruta)
        {
            var userId = User.FindFirst("UserId")?.Value;

            if (userId == null)
            {
                return BadRequest(new Respuesta { Exito = false, Mensaje = "ID de usuario es requerido." });
            }

            var respuesta = await _r2Service.ObtenerArchivoAsync(userId, titulo, ruta) as OkObjectResult;
            var respuesta2 = await _r2Service.ObtenerCantidadMbUsuario(userId);
            var okResult = respuesta2 as OkObjectResult;
            var respuesta3 = await _documentosRepository.ObtenerDocumentosActivosUltimo4(userId);


            Console.WriteLine(respuesta3);

            if (respuesta is OkObjectResult)
            {
                Console.WriteLine(respuesta2);
                return Ok(new { archivos = respuesta, archivosTamaño = okResult.Value, documentosActivos = respuesta3 });
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

        [HttpGet]
        [Route("documentosTipos")]
        public async Task<IActionResult> ObtenerTiposDocumentos()
        {
            var userId = User.FindFirst("UserId")?.Value;

            var respuesta = await _documentosRepository.ObtenerTiposDocumentos(userId);

            if (respuesta.Exito)
            {
                return Ok(respuesta);
            }
            return BadRequest(respuesta);
        }


    }

}
