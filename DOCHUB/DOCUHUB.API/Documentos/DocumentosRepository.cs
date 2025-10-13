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
            Console.WriteLine($"Subiendo documento para Usuario: {idUsuario}, Nombre Archivo: {nombreArchivo}, Ruta Archivo: {rutaArchivo}");
            var parametros = DocumentosDB.SubirDocumentosParams(idUsuario, nombreArchivo, rutaArchivo);

            using (var conexion = CrearConexion())
            {
                var resultado = await conexion.ExecuteAsync(DocumentosDB.spRegistroUsuario, parametros, commandType: CommandType.StoredProcedure);
                if (resultado > 0)
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


        public async Task<RespuestaDocumentos> ObtenerDocumentos(string idUsuario)
        {
            try
            {
                var documentos = await _context.DocumentoHistorial
                    .Where(d => d.idUsuario == int.Parse(idUsuario))
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

        public async Task<RespuestaDocumentos> ObtenerDocumentosActivos(string idUsuario)
        {
            try
            {
                var documentos = await _context.DocumentoHistorial
                    .Where(d => d.idUsuario == int.Parse(idUsuario) && d.idEstado == 1)
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

        public async Task<Respuesta> EliminarDocumento(string idUsuario, string titulo, string fechaCarga)
        {
            try
            {
                var listaTitulos = titulo.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                         .Select(t => t.Trim())
                                         .ToList();

                var listaFechaCarga = fechaCarga.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                .Select(f => DateTime.Parse(f.Trim()))
                                                .ToList();

                var fechasFormateadas = string.Join(",", listaFechaCarga.Select(f => f.ToString("yyyy-MM-dd HH:mm:ss")));

                var titulosSeparados = string.Join(",", listaTitulos);

                var parametros = DocumentosDB.EliminarDocumentoParams(idUsuario, titulosSeparados, fechasFormateadas);
                Console.WriteLine("Parámetros enviados: " + parametros);

                using (var conexion = CrearConexion())
                {
                    var resultado = await conexion.ExecuteAsync(
                        DocumentosDB.sp_EliminarDocumento,
                        parametros,
                        commandType: CommandType.StoredProcedure
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
    }
}
