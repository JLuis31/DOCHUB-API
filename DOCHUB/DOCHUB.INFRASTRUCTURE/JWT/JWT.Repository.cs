using DOCHUB.APP.JWT;
using Microsoft.EntityFrameworkCore;
using DOCHUB.APP.Data;

namespace DOCHUB.APP.Repositories
{
    public class JWTRepository
    {

        private readonly RefreshToken _refreshToken;
        private readonly AccessToken _accessToken;
        private readonly AppDBContext _context;
        private readonly IConfiguration _configuration;

        public JWTRepository(RefreshToken refreshToken, AccessToken accessToken, AppDBContext context, IConfiguration configuration)
        {
            _refreshToken = refreshToken;
            _accessToken = accessToken;
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> InsercionRefreshToken(string email)
        {
            var refreshToken = await _refreshToken.GenerateRefreshToken();
            var insertarRefreshToken = await _context.Usuarios.Where(u => u.Email == email).FirstOrDefaultAsync();
            insertarRefreshToken.RefreshToken = refreshToken;
            insertarRefreshToken.RefreshTokenExpiryTime = DateTime.Now.AddDays(_configuration.GetValue<int>("JWT:RefreshTokenExpirationDays"));
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<string> ActualizacionAccesToken(string refreshToken)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (usuario == null || usuario.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return null;
            }

            var nuevoAccessToken = await _accessToken.AccesToken(usuario.Id, _configuration["JWT:SecretKey"], _configuration["JWT:Issuer"], _configuration["JWT:Audience"], _configuration.GetValue<int>("JWT:AccessTokenExpirationMinutes"));

            return nuevoAccessToken;
        }

        public async Task<bool> ValidacionRefreshToken(string refreshToken)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (usuario == null || usuario.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return false;
            }

            return true;

        }

    }
}