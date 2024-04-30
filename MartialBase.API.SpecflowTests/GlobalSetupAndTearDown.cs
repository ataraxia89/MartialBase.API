// <copyright file="GlobalSetupAndTearDown.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.SpecflowTests
// Copyright © Martialtech®. All rights reserved.
// </copyright>

using DotNet.Testcontainers.Containers;

using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools;
using MartialBase.API.TestTools.Docker;
using MartialBase.API.TestTools.Docker.Models;
using MartialBase.API.TestTools.TestResources;
using MartialBase.API.Tools;

using Microsoft.Extensions.Configuration;

using TechTalk.SpecFlow;

using Testcontainers.MsSql;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.SpecflowTests
{
    [Binding]
    public static class GlobalSetupAndTearDown
    {
        public static readonly string ApiContainer = "ApiContainer";
        public static readonly string Configuration = "Configuration";
        public static readonly string CreatedSqlContainer = "CreatedSqlContainer";
        public static readonly string DbIdentifier = "DbIdentifier";
        public static readonly string HttpClient = "HttpClient";
        public static readonly string CurrentUser = "CurrentUser";

        /// <summary>
        /// Automation logic that has to run before the entire test run.
        /// </summary>
        [BeforeTestRun]
        public static void Setup()
        {
            // Method intentionally left empty.
        }

        /// <summary>
        /// Automation logic that has to run before executing each feature.
        /// </summary>
        /// <param name="featureContext">The current FeatureContext instance.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [BeforeFeature]
        public static async Task BeforeFeatureAsync(FeatureContext featureContext)
        {
            if (featureContext == null)
            {
                throw new ArgumentNullException(nameof(featureContext));
            }

            Console.WriteLine($"Starting {featureContext.FeatureInfo.Title} feature...");

            string testIdentifier = RandomData.GetRandomString(5);

            featureContext["Configuration"] = Data.Utilities.GetConfiguration("appsettings.TestingSQLServer.json");

            await PrepareSqlContainerAsync(featureContext, testIdentifier);
            await PrepareApiContainerAsync(featureContext, testIdentifier);
        }

        [BeforeScenario]
        public static async Task BeforeScenarioAsync(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            if (featureContext == null)
            {
                throw new ArgumentNullException(nameof(featureContext));
            }

            if (scenarioContext == null)
            {
                throw new ArgumentNullException(nameof(scenarioContext));
            }

            Console.WriteLine($"Starting {scenarioContext.ScenarioInfo.Title} feature...");

            await PrepareTestUserAsync(featureContext, scenarioContext);
            PrepareHttpClient(featureContext, scenarioContext);
        }

        [AfterScenario]
        public static void AfterScenario(ScenarioContext scenarioContext)
        {
            var httpClient = (HttpClient)scenarioContext["HttpClient"];
            httpClient.Dispose();
        }

        /// <summary>
        /// Automation logic that has to run after executing each feature.
        /// </summary>
        /// <param name="featureContext">The current FeatureContext instance.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [AfterFeature]
        public static async Task AfterFeatureAsync(FeatureContext featureContext)
        {
            if (featureContext == null)
            {
                throw new ArgumentNullException(nameof(featureContext));
            }

            Console.WriteLine($"Finishing {featureContext.FeatureInfo.Title} feature...");

            MsSqlContainer sqlContainer = ((CreatedSqlContainer)featureContext[CreatedSqlContainer]).SqlContainer;
            var apiContainer = (IContainer)featureContext["ApiContainer"];

            await apiContainer.StopAsync();
            await apiContainer.DisposeAsync();
            await sqlContainer.StopAsync();
            await sqlContainer.DisposeAsync();
        }

        /// <summary>
        /// Automation logic that has to run after the entire test run.
        /// </summary>
        [AfterTestRun]
        public static void TearDown()
        {
            // Method intentionally left empty.
        }

        private static async Task PrepareSqlContainerAsync(FeatureContext featureContext, string testIdentifier)
        {
            string featureName = featureContext.FeatureInfo.Title;

            CreatedSqlContainer createdSqlContainer = await ContainerTools.CreateAndStartSqlContainerAsync(
                $"{featureName}FeatureSql-{testIdentifier}",
                $"{featureName}Network-{testIdentifier}");

            featureContext[CreatedSqlContainer] = createdSqlContainer;

            string hostConnectionString = createdSqlContainer.ConnectionString.Replace(
                createdSqlContainer.NetworkAlias,
                $"localhost,{createdSqlContainer.SqlContainer.GetMappedPublicPort(1433)}");

            featureContext[DbIdentifier] = $"{featureName}-{testIdentifier}";

            DatabaseTools.RegisterMartialBaseDbContext(
                featureContext[DbIdentifier].ToString(),
                (IConfiguration)featureContext[Configuration],
                hostConnectionString);
        }

        private static async Task PrepareApiContainerAsync(FeatureContext featureContext, string testIdentifier)
        {
            var sqlContainer = (CreatedSqlContainer)featureContext[CreatedSqlContainer];

            featureContext[ApiContainer] = await ContainerTools.CreateAndStartContainerAsync(
                $"martialbase-api/martialbase-api:{testIdentifier}",
                $"{featureContext.FeatureInfo.Title}FeatureApi-{testIdentifier}",
                sqlContainer.Network,
                sqlContainer.SqlContainer,
                sqlContainer.ConnectionString);
        }

        private static async Task PrepareTestUserAsync(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            MartialBaseUser testUser =
                await MartialBaseUserResources.CreateMartialBaseUserAsync(featureContext[DbIdentifier].ToString());

            await MartialBaseUserRoleResources.EnsureUserHasRolesAsync(
                testUser.Id,
                UserRoles.GetRoles
                    .Select(ur => ur.Name),
                featureContext[DbIdentifier].ToString());

            scenarioContext[CurrentUser] = testUser;
        }

        private static void PrepareHttpClient(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(
                    $"http://localhost:{((IContainer)featureContext[ApiContainer]).GetMappedPublicPort(80)}")
            };

            client.DefaultRequestHeaders.Add(
                "Authorization",
                $"Bearer {AuthTokens.GenerateAuthorizationToken(
                    (IConfiguration)featureContext[Configuration],
                    (Guid)((MartialBaseUser)scenarioContext[CurrentUser]).AzureId)}");

            scenarioContext[HttpClient] = client;
        }
    }
}
