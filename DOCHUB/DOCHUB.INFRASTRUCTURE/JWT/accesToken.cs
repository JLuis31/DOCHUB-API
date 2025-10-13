using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DOCHUB.APP.Models;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.IdentityModel.Tokens;

namespace DOCHUB.APP.JWT
{
    public class AccessToken
    {

        public async Task<string> AccesToken(int usuarioId, string secretKey, string issuer, string audience, int expirationMinutes)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new ClaimsIdentity(new[]
            {
                new Claim("UserId", usuarioId.ToString()),
                new Claim(ClaimTypes.Role, "User")
            });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Subject = claims,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}