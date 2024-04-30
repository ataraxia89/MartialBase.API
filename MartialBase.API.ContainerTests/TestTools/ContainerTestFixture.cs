// <copyright file="TestServerFixture.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveControllerTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using DotNet.Testcontainers.Containers;

using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools;
using MartialBase.API.TestTools.Docker;
using MartialBase.API.TestTools.TestResources;
using MartialBase.API.Tools;

using Testcontainers.MsSql;

using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.ContainerTests.TestTools
{
    public sealed class ContainerTestFixture : IAsyncLifetime, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly string _identifier;
        
        private IContainer _apiContainer;
        private MsSqlContainer _sqlContainer;
        private MartialBaseUser _testUser;

        /// <summary>
        /// This is ran once before all test classes implementing this fixture.
        /// </summary>
        public ContainerTestFixture()
        {
            _identifier = RandomData.GetRandomString(5);

            _configuration = Data.Utilities.GetConfiguration("appsettings.TestingSQLServer.json");

            DbIdentifier = $"ContainerTests-{Guid.NewGuid():N}";
        }

        public string DbIdentifier { get; }

        public HttpClient Client { get; private set; }

        public MartialBaseUser TestUser => _testUser;

        /// <summary>
        /// This is ran after the constructor, once before all test classes implementing this feature.
        /// </summary>
        public async Task InitializeAsync()
        {
            var createdSqlContainer = await ContainerTools.CreateAndStartSqlContainerAsync($"MartialBaseSqlTest-{_identifier}", $"MartialBaseNetwork-{_identifier}");

            _sqlContainer = createdSqlContainer.SqlContainer;

            _apiContainer = await ContainerTools.CreateAndStartContainerAsync(
                "martialbase-api/martialbase-api:latest",
                $"MartialBaseApiTest-{_identifier}",
                createdSqlContainer.Network,
                _sqlContainer,
                createdSqlContainer.ConnectionString);

            var hostConnectionString = createdSqlContainer.ConnectionString.Replace(
                createdSqlContainer.NetworkAlias,
                $"localhost,{_sqlContainer.GetMappedPublicPort(1433)}");

            DatabaseTools.RegisterMartialBaseDbContext(
                DbIdentifier,
                _configuration,
                hostConnectionString);

            _testUser = await MartialBaseUserResources.CreateMartialBaseUserAsync(DbIdentifier);

            await MartialBaseUserRoleResources.EnsureUserHasRolesAsync(
                _testUser.Id,
                UserRoles.GetRoles.Select(ur => ur.Name),
                DbIdentifier);

            Client = new HttpClient { BaseAddress = new Uri($"http://localhost:{_apiContainer.GetMappedPublicPort(80)}") };

            GenerateAuthorizationToken((Guid)_testUser.AzureId);
        }

        public void GenerateAuthorizationToken(Guid azureId)
        {
            ClearAuthorizationToken();

            Client.DefaultRequestHeaders.Add(
                "Authorization",
                $"Bearer {AuthTokens.GenerateAuthorizationToken(_configuration, azureId)}");
        }

        public void ClearAuthorizationToken()
        {
            Client.DefaultRequestHeaders.Remove("Authorization");
        }

        /// <summary>
        /// This runs just before the Dispose method below, once all tests implementing this fixture are complete.
        /// </summary>
        public async Task DisposeAsync()
        {
            await _apiContainer.StopAsync();
            await _apiContainer.DisposeAsync();
            await _sqlContainer.StopAsync();
            await _sqlContainer.DisposeAsync();
        }

        public void Dispose()
        {
            Client.Dispose();
        }
    }
}
