using DOCHUB.APP.ModelsDto;
using System.Data;
using Microsoft.Data.SqlClient;
using DOCHUB.APP.DB;
using Dapper;
using Microsoft.Extensions.Configuration;
using DOCHUB.APP.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using DOCHUB.APP.JWT;
using Azure;
using DOCHUB.APP.Base;

namespace DOCHUB.APP.Repositories
{
    public class UsuariosRepository : BaseRepository
    {
        private readonly AppDBContext _context;
        private readonly AccessToken _accessToken;
        private readonly IConfiguration _configuration;

        public UsuariosRepository(IConfiguration configuration, AppDBContext context, AccessToken accessToken, RefreshToken refreshToken) : base(configuration)
        {
            _context = context;
            _accessToken = accessToken;
            _configuration = configuration;
        }

        public async Task<Respuesta> RegistroUsuario(UsuarioDTO usuarioDTO)
        {
            var validacion = await _context.Usuarios.AnyAsync(u => u.Email == usuarioDTO.Email);
            if (validacion == true)
            {
                return new Respuesta
                {
                    Exito = false,
                    Mensaje = "El correo electrónico ya está registrado."
                };
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(usuarioDTO.Password);


            var usuarioConHashedPassword = new UsuarioDTO
            {
                Nombre = usuarioDTO.Nombre,
                Email = usuarioDTO.Email,
                Password = hashedPassword,
                RefreshToken = usuarioDTO.RefreshToken,
                RefreshTokenExpiryTime = usuarioDTO.RefreshTokenExpiryTime
            };


            var parametros = UsuariosDB.RegistroUsuarioParams(usuarioConHashedPassword);
            using (var conexion = CrearConexion())
            {
                var resultado = await conexion.ExecuteAsync(UsuariosDB.spRegistroUsuario, parametros, commandType: CommandType.StoredProcedure);
                if (resultado > 0)
                {
                    return new Respuesta
                    {
                        Exito = true,
                        Mensaje = "Usuario registrado exitosamente."
                    };
                }
                else
                {
                    return new Respuesta
                    {
                        Exito = false,
                        Mensaje = "Error al registrar el usuario."
                    };
                }
            }
        }

        public async Task<Respuesta> Login(string email, string password)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            ;

            if (usuario != null)
            {


                bool passwordValida = BCrypt.Net.BCrypt.Verify(password, usuario.Password);

                if (passwordValida)
                {
                    var accessToken = await _accessToken.AccesToken(usuario.Id, _configuration["JWT:SecretKey"], _configuration["JWT:Issuer"], _configuration["JWT:Audience"], _configuration.GetValue<int>("JWT:AccessTokenExpirationMinutes"));

                    return new Respuesta
                    {
                        Exito = true,
                        Mensaje = "Inicio de sesión exitoso.",
                        Id = usuario.Id,
                        Token = accessToken
                    };
                }
            }
            else
            {
                Console.WriteLine($"Usuario no encontrado con email: {email}");
            }

            return new Respuesta
            {
                Exito = false,
                Mensaje = "Credenciales inválidas."
            };
        }


    }
}