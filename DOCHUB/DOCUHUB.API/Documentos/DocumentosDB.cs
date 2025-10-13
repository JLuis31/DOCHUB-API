namespace DOCHUB.APP.DB
{
    public class DocumentosDB
    {

        internal static string sp_EliminarDocumento = "sp_EliminarDocumento";
        internal static string spRegistroUsuario = "sp_InsercionDocumentos";

        internal static object SubirDocumentosParams(string idUsuario, string titulo, string ruta)
        {
            var parametros = new
            {
                idUsuario,
                titulo,
                ruta
            };

            return parametros;
        }

        internal static object EliminarDocumentoParams(string idUsuario, string titulo, string fechaCarga)
        {
            var parametros = new
            {
                idUsuario,
                titulo,
                fechaCarga
            };

            return parametros;
        }
    }
}