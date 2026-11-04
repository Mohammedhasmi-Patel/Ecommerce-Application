using Ecommerce.Configurations;
using Ecommerce.Contracts;
using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ecommerce.Services.Auth;

public class TokenService : ITokenService
{
    private readonly JwtSetting _jwtSetting;
    private readonly UserManager<AspNetUsers> _userManager;

    public TokenService(IOptions<JwtSetting> jwtSetting, UserManager<AspNetUsers> userManager)
    {
        _jwtSetting = jwtSetting.Value;
        _userManager = userManager;

    }

    public async Task<string> GenerateToken(AspNetUsers user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

        if (role is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: _jwtSetting.Issuer,
            audience: _jwtSetting.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);

    }
}
