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
                @Nombre = usuario.Nombre,
                @Email = usuario.email,
                @Password = usuario.Password,
                @RefreshToken = usuario.RefreshToken,
                @RefreshTokenExpiryTime = usuario.RefreshTokenExpiryTime
            };

            return parametros;
        }
    }
}