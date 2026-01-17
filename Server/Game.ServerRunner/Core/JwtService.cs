namespace Game.ServerRunner.Core;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class JwtService(string issuer, string audience, TimeSpan tokenLifetime, string signingKey, string environment) {
    private readonly SecurityKey signingSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

    public TokenValidationParameters GetTokenValidationParameters() {
        return new TokenValidationParameters {
            ValidateIssuer = true,
            ValidIssuer    = issuer,

            ValidateAudience = true,
            ValidAudience    = audience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = this.signingSecurityKey,
        };
    }

    public string GetAccessToken(Guid userId) {
        var now = DateTime.UtcNow;
        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            notBefore: now,
            claims: [
                new Claim(JwtRegisteredClaimNames.NameId, userId.ToString()),
                new Claim("env", environment ?? string.Empty),
            ],
            expires: now.Add(tokenLifetime),
            signingCredentials: new SigningCredentials(this.signingSecurityKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}