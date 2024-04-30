// <copyright file="TestServerFixture.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveControllerTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

#pragma warning disable IDE0005 // Using directive is unnecessary.
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;

using MartialBase.API.AuthTools;
using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Data;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.TestTools;
using MartialBase.API.TestTools.TestResources;
using MartialBase.API.Tools;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using Xunit;
#pragma warning restore IDE0005 // Using directive is unnecessary.

namespace MartialBase.API.LiveControllerTests.TestTools
{
    public sealed class TestServerFixture : IDisposable
    {
        private readonly TestServer _testServer;
        private readonly string _dbIdentifier = "MartialBaseTest";
        private MartialBaseUser _testUser;
        private DateTime _tokenExpiry;
        private Guid? _azureId;
        private string _invitationCode;
        private IConfiguration _configuration;

        public TestServerFixture()
        {
#if !TESTING
            throw new NotSupportedException("Live controller tests can only be run in Testing configuration.");
#endif
            _configuration = Data.Utilities.GetConfiguration("appsettings.Debug.json");

            DatabaseTools.RegisterMartialBaseDbContext(DbIdentifier, _configuration);

            // Create web host
            IWebHostBuilder builder = new WebHostBuilder()
                .UseContentRoot(GetContentRootPath())
                .UseEnvironment("Testing")
                .UseConfiguration(_configuration)
                .UseStartup<Startup>();

            // Start server
            _testServer = new TestServer(builder);
            Client = _testServer.CreateClient();
        }

        public HttpClient Client { get; }

        public MartialBaseUser TestUser => _testUser;

        public DateTime AuthTokenExpiry => _tokenExpiry;

        public Guid? AzureId => _azureId;

        public string DbIdentifier => _dbIdentifier;

        public string InvitationCode => _invitationCode;

        public void GenerateAuthorizationToken(int expiryMinutes = 20)
        {
            ClearAuthorizationToken();

            Assert.NotNull(_azureId);

            var claims = new List<Claim>
            {
                new Claim(ClaimConstants.Scp, "query"),
                new Claim("aud", _configuration["JWT:ValidAudience"]),
                new Claim(ClaimConstants.Oid, _azureId.ToString())
            };

            if (_invitationCode != null)
            {
                claims.Add(new Claim("extension_InvitationCode", _invitationCode));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            _tokenExpiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var token = new JwtSecurityToken(
                _configuration["JWT:ValidIssuer"],
                _configuration["JWT:ValidAudience"],
                claims,
                null,
                _tokenExpiry,
                signingCredentials);

            var tokenStr = tokenHandler.WriteToken(token);

            Client.DefaultRequestHeaders.Add(
                "Authorization",
                $"Bearer {tokenStr}");
        }

        public void ClearAuthorizationToken()
        {
            Client.DefaultRequestHeaders.Remove("Authorization");
        }

        public void SetUpUnitTest()
        {
            _testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            _azureId = _testUser.AzureId;
            _invitationCode = _testUser.InvitationCode;

            GenerateAuthorizationToken();

            CountryResources.ClearUsedCountries();
        }

        public void RemoveTestUser()
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(DbIdentifier))
            {
                dbContext.MartialBaseUsers.Remove(dbContext.MartialBaseUsers.First(mbu => mbu.Id == _testUser.Id));

                Assert.True(dbContext.SaveChanges() > 0);
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(DbIdentifier))
            {
                Assert.False(dbContext.MartialBaseUsers.Any(mbu => mbu.Id == _testUser.Id));
            }

            _testUser = null;
        }

        public void BlockTestUserAccess()
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(DbIdentifier))
            {
                MartialBaseUser user = dbContext.MartialBaseUsers.First(mbu => mbu.Id == _testUser.Id);

                user.AzureId = null;
                user.InvitationCode = null;

                Assert.True(dbContext.SaveChanges() > 0);
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(DbIdentifier))
            {
                Assert.True(dbContext.MartialBaseUsers.Any(mbu =>
                    mbu.Id == _testUser.Id &&
                    mbu.AzureId == null &&
                    mbu.InvitationCode == null));
            }

            _testUser.AzureId = null;
            _testUser.InvitationCode = null;
        }

        public void RemoveAzureIdFromDbTestUser()
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(DbIdentifier))
            {
                MartialBaseUser user = dbContext.MartialBaseUsers.First(mbu => mbu.Id == _testUser.Id);

                user.AzureId = null;

                Assert.True(dbContext.SaveChanges() > 0);
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(DbIdentifier))
            {
                Assert.True(dbContext.MartialBaseUsers.Any(mbu =>
                    mbu.Id == _testUser.Id &&
                    mbu.AzureId == null));
            }
        }

        public void Dispose()
        {
            Client.Dispose();
            _testServer.Dispose();
            DatabaseTools.DeleteDatabase(DbIdentifier);
        }

        private static string GetContentRootPath()
        {
            string testProjectPath = PlatformServices.Default.Application.ApplicationBasePath;
            string relativePathToHostProject = @"..\..\..\..\MartialBase.API";

            return Path.Combine(testProjectPath, relativePathToHostProject);
        }
    }
}
