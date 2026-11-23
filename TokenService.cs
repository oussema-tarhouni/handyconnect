using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HandyConnect.Api.Models;
using HandyConnect.Api.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HandyConnect.Api.Services;

public sealed class TokenService(IOptions<JwtOptions> options)
{
    public string Create(UserRow user)
    {
        var jwt = options.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
        var claims = new[]
        {
            new Claim("uid", user.Id.ToString()),
            new Claim("name", user.FullName),
            new Claim("role", user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwt.ExpiresMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
