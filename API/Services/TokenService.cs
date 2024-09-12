using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    public string CreateToken(AppUser user)
    {
        var tokenKey = config["TokenKey"] ?? throw new Exception("Cannot access token key from AppSettings");
        if (tokenKey.Length < 64) throw new Exception("Token key needs to be longer");
        
        // Symmetric key uses the same key to encrypt and decrypt
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        // Verify claims
        var claims = new List<Claim> 
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserName)
        };

        // Sign the JWT using our token key to let us know our server approved the claim
        // HmacSha512 requires a 64-length token key for the algorithm
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
