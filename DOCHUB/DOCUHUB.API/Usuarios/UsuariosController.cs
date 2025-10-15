using Microsoft.AspNetCore.Mvc;
using DOCHUB.APP.ModelsDto;
using DOCHUB.APP.Repositories;
using DOCHUB.APP.ModelsDto;

namespace DOCHUB.APP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class UsuariosController : ControllerBase
    {
        private readonly UsuariosRepository _usuariosRepository;
        private readonly JWTRepository _jwtRepository;
        private readonly IConfiguration _configuration;

        public UsuariosController(UsuariosRepository usuariosRepository, JWTRepository jwtRepository, IConfiguration configuration)
        {
            _usuariosRepository = usuariosRepository;
            _jwtRepository = jwtRepository;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> RegistroUsuario([FromBody] UsuarioDTO usuarioDTO)
        {

            var respuesta = await _usuariosRepository.RegistroUsuario(usuarioDTO);

            Console.WriteLine(respuesta.Mensaje);
            return Ok(respuesta);
        }

        [HttpPost]
        [Route("login")]

        public async Task<IActionResult> Login(string email, string password)
        {

            var respuesta = await _usuariosRepository.Login(email, password);
            var refreshToken = await _jwtRepository.InsercionRefreshToken(email);
            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(_configuration.GetValue<int>("JWT:RefreshTokenExpirationDays"))
            });
            return Ok(respuesta);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("refreshToken");
            return Ok(new { message = "Sesión cerrada" });
        }


        [HttpPost]
        [Route("refresh-token")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<Respuesta>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var estadoRefreshtToken = await _jwtRepository.ValidacionRefreshToken(refreshToken);
            if (estadoRefreshtToken == true)
            {
                var nuevoAccessToken = await _jwtRepository.ActualizacionAccesToken(refreshToken);
                return Ok(new Respuesta
                {
                    Exito = true,
                    Mensaje = "Nuevo access token generado.",
                    Token = nuevoAccessToken
                });
            }
            return Unauthorized(new Respuesta
            {
                Exito = false,
                Mensaje = "Refresh token inválido o expirado."
            });
        }
    }
}