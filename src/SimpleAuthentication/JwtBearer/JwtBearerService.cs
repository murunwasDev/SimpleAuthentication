﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SimpleAuthentication.JwtBearer;

internal class JwtBearerService : IJwtBearerService
{
    private readonly JwtBearerSettings jwtBearerSettings;

    public JwtBearerService(IOptions<JwtBearerSettings> jwtBearerSettingsOptions)
    {
        jwtBearerSettings = jwtBearerSettingsOptions.Value;
    }

    public string CreateToken(string userName, IList<Claim>? claims = null, string? issuer = null, string? audience = null, DateTime? absoluteExpiration = null)
    {
        claims ??= new List<Claim>();
        claims.Update(ClaimTypes.Name, userName);

        var now = DateTime.UtcNow;

        var jwtSecurityToken = new JwtSecurityToken(
            issuer ?? jwtBearerSettings.Issuers?.FirstOrDefault(),
            audience ?? jwtBearerSettings.Audiences?.FirstOrDefault(),
            claims,
            now,
            absoluteExpiration ?? (jwtBearerSettings.ExpirationTime.GetValueOrDefault() > TimeSpan.Zero ? now.Add(jwtBearerSettings.ExpirationTime!.Value) : DateTime.MaxValue),
            new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtBearerSettings.SecurityKey)), jwtBearerSettings.Algorithm));

        var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        return token;
    }
}
