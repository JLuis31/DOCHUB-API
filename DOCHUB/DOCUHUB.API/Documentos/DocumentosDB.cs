namespace DOCHUB.APP.DB
{
    public class DocumentosDB
    {

        internal static string sp_EliminarDocumento = "sp_eliminardocumento";
        internal static string sp_SubirDocumento = "sp_inserciondocumentos";

        internal static object SubirDocumentosParams(string idUsuario, string titulo, string ruta)
        {
            var parametros = new
            {
                @idUsuario,
                @titulo,
                @ruta
            };

            return parametros;
        }

        internal static object EliminarDocumentoParams(string idUsuario, string titulo)
        {
            var parametros = new
            {
                @idUsuario,
                @titulo
            };

            return parametros;
        }
    }
}