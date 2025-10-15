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
            var validacion = await _context.usuarios.AnyAsync(u => u.email == usuarioDTO.email);
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
                email = usuarioDTO.email,
                Password = hashedPassword,
                RefreshToken = usuarioDTO.RefreshToken,
                RefreshTokenExpiryTime = usuarioDTO.RefreshTokenExpiryTime
            };


            var parametros = UsuariosDB.RegistroUsuarioParams(usuarioConHashedPassword);
            using (var conexion = CrearConexion())
            {
                var resultado = await conexion.ExecuteScalarAsync<int>(
                    $"Select {UsuariosDB.spRegistroUsuario}(@Nombre, @Email, @Password, @RefreshToken, @RefreshTokenExpiryTime);", parametros, commandType: CommandType.Text
                );
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
            var usuario = await _context.usuarios.FirstOrDefaultAsync(u => u.email == email);
            ;

            if (usuario != null)
            {


                bool passwordValida = BCrypt.Net.BCrypt.Verify(password, usuario.password);

                if (passwordValida)
                {
                    var accessToken = await _accessToken.AccesToken(usuario.id, _configuration["JWT:SecretKey"], _configuration["JWT:Issuer"], _configuration["JWT:Audience"], _configuration.GetValue<int>("JWT:AccessTokenExpirationMinutes"));

                    return new Respuesta
                    {
                        Exito = true,
                        Mensaje = "Inicio de sesión exitoso.",
                        Id = usuario.id,
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