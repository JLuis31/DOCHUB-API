using DOCHUB.APP.ModelsDto;
namespace DOCHUB.APP.DB
{

    public class UsuariosDB
    {
        internal static string spRegistroUsuario = "sp_RegistrarUsuario";

        internal static object RegistroUsuarioParams(UsuarioDTO usuario)
        {
            var parametros = new
            {
                usuario.Nombre,
                usuario.Email,
                usuario.Password,
                usuario.RefreshToken,
                usuario.RefreshTokenExpiryTime
            };

            return parametros;
        }
    }
}