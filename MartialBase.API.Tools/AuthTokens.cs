// <copyright file="AuthTokens.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Tools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

using Microsoft.Identity.Web;

using Microsoft.IdentityModel.Tokens;

namespace MartialBase.API.Tools
{
    public static class AuthTokens
    {
        public static string GenerateAuthorizationToken(IConfiguration configuration, Guid azureId, int expiryMinutes = 20)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimConstants.Scp, "query"),
                new Claim("aud", configuration["JWT:ValidAudience"]),
                new Claim(ClaimConstants.Oid, azureId.ToString())
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                configuration["JWT:ValidIssuer"],
                configuration["JWT:ValidAudience"],
                claims,
                null,
                DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials);

            var tokenStr = tokenHandler.WriteToken(token);

            return tokenStr;
        }

        public static string GenerateAuthorizationToken(IConfiguration configuration, Guid azureId, string invitationCode, int expiryMinutes = 20)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimConstants.Scp, "query"),
                new Claim("aud", configuration["JWT:ValidAudience"]),
                new Claim("extension_InvitationCode", invitationCode),
                new Claim(ClaimConstants.Oid, azureId.ToString())
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                configuration["JWT:ValidIssuer"],
                configuration["JWT:ValidAudience"],
                claims,
                null,
                DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials);

            var tokenStr = tokenHandler.WriteToken(token);

            return tokenStr;
        }
    }
}
