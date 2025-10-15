using Minio;
using Minio.DataModel;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using Minio.DataModel.Args;
using Microsoft.AspNetCore.Mvc;
using Minio.ApiEndpoints;

namespace DOCHUB.Infrastructure.Storage
{
    public class CloudflareR2MinioService
    {
        private readonly IMinioClient _minio;
        private readonly string? _bucketName;
        private readonly string? _accountId;

        public CloudflareR2MinioService(IConfiguration config)
        {
            var r2 = config.GetSection("CloudflareR2");
            _bucketName = r2["BucketNamePublicado"];
            _accountId = r2["AccountIdPublicado"];

            _minio = new MinioClient()
                .WithEndpoint($"{_accountId}.r2.cloudflarestorage.com")
                .WithCredentials(r2["AccessKeyIdPublicado"], r2["SecretAccessKeyPublicado"])
                .WithSSL()
                .Build();
        }

        public async Task<string> SubirArchivoAsync(string idUsuario, IFormFile archivo, string keyDestino)
        {
            using var stream = archivo.OpenReadStream();
            keyDestino = $"{idUsuario}/{keyDestino}";

            await _minio.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(keyDestino)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(archivo.ContentType)
            );

            return $"https://{_bucketName}.{_accountId}.r2.dev/{keyDestino}";
        }

        public async Task EliminarArchivoAsync(string idUsuario, string key)
        {
            var archivos = key.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(t => t.Trim());

            foreach (var archivo in archivos)
            {
                string objetoPath = $"{idUsuario}/{archivo}";
                Console.WriteLine($"Intentando eliminar: {objetoPath}");
                try
                {
                    await _minio.RemoveObjectAsync(new RemoveObjectArgs()
                        .WithBucket(_bucketName)
                        .WithObject(objetoPath));
                    Console.WriteLine("Eliminación exitosa.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al eliminar {objetoPath}: {ex.Message}");
                }
            }
        }

        public async Task<ActionResult> ObtenerArchivoAsync(string idUsuario, string key, string rutaDestino)
        {
            if (string.IsNullOrWhiteSpace(idUsuario) || string.IsNullOrWhiteSpace(key))
                return new BadRequestObjectResult("Los parámetros idUsuario y key son obligatorios.");

            var keys = key.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(k => k.Trim())
                          .ToArray();

            if (keys.Length > 4)
                keys = keys.Skip(keys.Length - 4).ToArray();

            var rutas = rutaDestino?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(r => r.Trim())
                                    .ToArray() ?? Array.Empty<string>();

            var resultados = new List<object>();

            for (int i = 0; i < keys.Length; i++)
            {
                var trimmedKey = keys[i];
                var fileName = Path.GetFileName(trimmedKey);

                try
                {
                    using var memoryStream = new MemoryStream();

                    await _minio.GetObjectAsync(new GetObjectArgs()
                        .WithBucket(_bucketName)
                        .WithObject($"{idUsuario}/{trimmedKey}")
                        .WithCallbackStream(stream =>
                        {
                            stream.CopyTo(memoryStream);
                        }));

                    resultados.Add(new
                    {
                        archivo = fileName,
                        rutaDestino = rutas.Length == keys.Length ? rutas[i] : (rutas.Length > 0 ? rutas[0] : null),
                        tamano = memoryStream.Length,
                        contenidoBase64 = Convert.ToBase64String(memoryStream.ToArray()),
                        estado = "OK"
                    });
                }
                catch (Exception ex)
                {
                    resultados.Add(new
                    {
                        archivo = fileName,
                        error = ex.Message,
                        estado = "Error"
                    });
                }
            }

            return new OkObjectResult(resultados);
        }

        public async Task<ActionResult> ObtenerTamañoArchivosUltimos4(string idUsuario, string key, string rutaDestino)
        {
            if (string.IsNullOrWhiteSpace(idUsuario) || string.IsNullOrWhiteSpace(key))
                return new BadRequestObjectResult("Los parámetros idUsuario y key son obligatorios.");

            var keys = key.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(k => k.Trim())
                          .ToArray();

            if (keys.Length > 4)
                keys = keys.Skip(keys.Length - 4).ToArray();

            var rutas = rutaDestino?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(r => r.Trim())
                                    .ToArray() ?? Array.Empty<string>();

            var resultados = new List<object>();

            for (int i = 0; i < keys.Length; i++)
            {
                var trimmedKey = keys[i];
                var fileName = Path.GetFileName(trimmedKey);

                try
                {
                    using var memoryStream = new MemoryStream();

                    await _minio.GetObjectAsync(new GetObjectArgs()
                        .WithBucket(_bucketName)
                        .WithObject($"{idUsuario}/{trimmedKey}")
                        .WithCallbackStream(stream =>
                        {
                            stream.CopyTo(memoryStream);
                        }));

                    resultados.Add(new
                    {
                        tamano = memoryStream.Length,
                    });
                }
                catch (Exception ex)
                {
                    resultados.Add(new
                    {
                        archivo = fileName,
                        error = ex.Message,
                        estado = "Error"
                    });
                }
            }

            return new OkObjectResult(resultados);
        }

        public async Task<ActionResult> ObtenerCantidadMbUsuario(string idUsuario)
        {
            long totalBytes = 0;
            var listadoObjetos = _minio.ListObjectsAsync(
                new ListObjectsArgs()
                    .WithBucket(_bucketName)
                    .WithPrefix($"{idUsuario}/")
                    .WithRecursive(true)
            ); var tcs = new TaskCompletionSource<long>();

            listadoObjetos.Subscribe(
                objeto => totalBytes += (long)objeto.Size,
                error => tcs.SetException(error),
                () => tcs.SetResult(totalBytes)
            );

            await tcs.Task;
            double totalMb = totalBytes / (1024.0 * 1024.0);
            return new OkObjectResult(new { megabytesUsados = totalMb });
        }
        public async Task<MemoryStream> DescargarArchivoAsync(string idUsuario, string nombreArchivo)
        {

            if (string.IsNullOrWhiteSpace(idUsuario) || string.IsNullOrWhiteSpace(nombreArchivo))
                throw new ArgumentException("El idUsuario y el nombreArchivo son obligatorios.");

            var memoryStream = new MemoryStream();

            await _minio.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject($"{idUsuario}/{nombreArchivo}")
                .WithCallbackStream(stream =>
                {
                    stream.CopyTo(memoryStream);
                }));

            memoryStream.Position = 0;
            return memoryStream;
        }




    }


}
