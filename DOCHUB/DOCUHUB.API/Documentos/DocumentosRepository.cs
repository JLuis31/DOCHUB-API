using System;
using DOCHUB.APP.DB;
using DOCHUB.APP.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DOCHUB.APP.ModelsDto;
using DOCHUB.APP.Models;
using System.Data;
using DOCHUB.APP.Base;
using Microsoft.Extensions.Configuration;
using Dapper;

namespace DOCHUB.APP.Repositories
{
    public class DocumentosRepository : BaseRepository
    {
        private readonly AppDBContext _context;

        public DocumentosRepository(AppDBContext context, IConfiguration configuration) : base(configuration)
        {
            _context = context;
        }

        public async Task<Respuesta> SubirDocumentos(string idUsuario, string nombreArchivo, string rutaArchivo)
        {
            var parametros = DocumentosDB.SubirDocumentosParams(idUsuario, nombreArchivo, rutaArchivo);

            using (var conexion = CrearConexion())
            {
                var resultado = await conexion.ExecuteScalarAsync<int>(
                    $"Select {DocumentosDB.sp_SubirDocumento}(@idUsuario, @titulo, @ruta);", parametros, commandType: CommandType.Text
                ); if (resultado > 0)
                {
                    return new Respuesta
                    {
                        Exito = true,
                        Mensaje = "Documento subido exitosamente."
                    };
                }
                return new Respuesta
                {
                    Exito = false,
                    Mensaje = "Error al subir el documento."
                };
            }
        }


        public async Task<RespuestaDocumentosDto> ObtenerDocumentos(string idUsuario)
        {
            try
            {
                var documentos = await _context.documentohistorial
                    .Where(d => d.idusuario == long.Parse(idUsuario))
                    .Select(d => new DocumentoHistorialDto
                    {
                        iddocumento = d.iddocumento,
                        idusuario = d.idusuario,
                        idestado = d.idestado.ToString(),
                        titulo = d.titulo,
                        ruta = d.ruta,
                        fechacarga = d.fechacarga
                    })
                    .ToListAsync();

                return new RespuestaDocumentosDto
                {
                    Exito = true,
                    Datos = documentos
                };
            }
            catch (Exception ex)
            {
                return new RespuestaDocumentosDto
                {
                    Exito = false,
                    Mensaje = $"Error al obtener los documentos: {ex.Message}"
                };
            }
        }

        public async Task<RespuestaDocumentos> ObtenerDocumentosActivos(string idUsuario)
        {
            try
            {
                long usuarioId = long.Parse(idUsuario);
                Console.WriteLine("ID de usuario recibido: " + usuarioId);

                var documentos = await _context.documentohistorial
                    .Where(d => d.idusuario == usuarioId && d.idestado == 1114885399933747201)
                    .ToListAsync();


                return new RespuestaDocumentos
                {
                    Exito = true,
                    Datos = documentos
                };
            }
            catch (Exception ex)
            {
                return new RespuestaDocumentos
                {
                    Exito = false,
                    Mensaje = $"Error al obtener los documentos: {ex.Message}"
                };
            }
        }

        public async Task<RespuestaDocumentos> ObtenerDocumentosActivosUltimo4(string idUsuario)
        {
            try
            {
                long usuarioId = long.Parse(idUsuario);
                var documentos = await _context.documentohistorial
                    .Where(d => d.idusuario == usuarioId)
                    .OrderByDescending(d => d.fechacarga)
                    .Take(4)
                    .ToListAsync();

                return new RespuestaDocumentos
                {
                    Exito = true,
                    Datos = documentos
                };
            }
            catch (Exception ex)
            {
                return new RespuestaDocumentos
                {
                    Exito = false,
                    Mensaje = $"Error al obtener los documentos: {ex.Message}"
                };
            }
        }

        public async Task<Respuesta> EliminarDocumento(string idUsuario, string titulo)
        {
            try
            {
                var listaTitulos = titulo.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                         .Select(t => t.Trim())
                                         .ToList();



                var titulosSeparados = string.Join(",", listaTitulos);

                var parametros = DocumentosDB.EliminarDocumentoParams(idUsuario, titulosSeparados);
                Console.WriteLine("Parámetros enviados: " + parametros);

                using (var conexion = CrearConexion())
                {
                    var resultado = await conexion.ExecuteScalarAsync<int>(
                        $"Select {DocumentosDB.sp_EliminarDocumento}(@idUsuario, @titulo);", parametros, commandType: CommandType.Text
                    );

                    if (resultado == 0)
                    {
                        return new Respuesta
                        {
                            Exito = false,
                            Mensaje = "No se encontró el documento o no tienes permisos para eliminarlo."
                        };
                    }
                }

                return new Respuesta
                {
                    Exito = true,
                    Mensaje = "Documento eliminado exitosamente."
                };
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    Exito = false,
                    Mensaje = $"Error al eliminar el documento: {ex.Message}"
                };
            }
        }


        public async Task<dynamic> ObtenerTiposDocumentos(string userId)
        {
            long usuarioId = long.Parse(userId);

            var tipoDocumentos = await _context.documentohistorial
                .Where(d => d.idusuario == usuarioId && !string.IsNullOrEmpty(d.ruta))
                .ToListAsync();

            var tipos = new List<string>();

            foreach (var doc in tipoDocumentos)
            {
                var extension = Path.GetExtension(doc.ruta)?.ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(extension))
                    tipos.Add(extension);
            }

            var tiposFormateados = string.Join(", ", tipos);
            return new
            {
                Exito = true,
                Datos = tiposFormateados
            };
        }
    }
}
