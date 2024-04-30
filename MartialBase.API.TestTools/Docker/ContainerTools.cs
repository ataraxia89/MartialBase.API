// <copyright file="ContainerTools.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Threading.Tasks;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

using MartialBase.API.TestTools.Docker.Models;
using MartialBase.API.Tools;

using Testcontainers.MsSql;

namespace MartialBase.API.TestTools.Docker
{
    public static class ContainerTools
    {
        public static async Task<CreatedSqlContainer> CreateAndStartSqlContainerAsync(string containerName, string networkAlias)
        {
            INetwork network = new NetworkBuilder().Build();

            string? dbPassword = RandomData.GetRandomString(30, true, true, true, @"!@$%~-");

            MsSqlContainer? sqlContainer = new MsSqlBuilder()
                .WithName(containerName)
                .WithNetwork(network)
                .WithNetworkAliases(networkAlias)
                .WithPortBinding(1433, true)
                .WithEnvironment("ACCEPT_EULA", "Y")
                .WithEnvironment("SQLCMDUSER", "sa")
                .WithEnvironment("SQLCMDPASSWORD", dbPassword)
                .WithEnvironment("MSSQL_SA_PASSWORD", dbPassword)
                .WithWaitStrategy(
                    Wait.ForUnixContainer().UntilCommandIsCompleted("/opt/mssql-tools/bin/sqlcmd", "-Q", "SELECT 1;"))
                .Build();

            await sqlContainer.StartAsync();

            string connectionString =
                $"server={networkAlias};user id={MsSqlBuilder.DefaultUsername};password={dbPassword};database={MsSqlBuilder.DefaultDatabase};Encrypt=false";

            return new CreatedSqlContainer(sqlContainer, connectionString, network, networkAlias);
        }

        public static async Task<IContainer> CreateAndStartContainerAsync(string imageName, string containerName, INetwork network, MsSqlContainer sqlContainer, string sqlConnectionString)
        {
            var image = new ImageFromDockerfileBuilder()
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
                .WithDockerfile("Dockerfile")
                .WithName(imageName)
                .Build();

            await image.CreateAsync();

            var container = new ContainerBuilder()
                .WithNetwork(network)
                .WithImage(image)
                .WithName(containerName)
                .DependsOn(sqlContainer)
                .WithEnvironment("ConnectionStrings__SQLConnection", sqlConnectionString)
                .WithPortBinding(80, true)
                .WithWaitStrategy(
                    Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(request => request.ForPath("health")))
                .Build();

            await container.StartAsync();

            return container;
        }
    }
}
